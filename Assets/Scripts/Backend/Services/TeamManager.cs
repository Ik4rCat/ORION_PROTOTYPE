using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Orion.Models;
using Orion.Backend.Services;
//using Newtonsoft.Json;
using Orion.Backend.Models;

namespace Orion.Services
{
    public class TeamManager : MonoBehaviour
    {
        public static TeamManager Instance { get; private set; }
        
        // Список команд (в реальном приложении будет загружаться из базы данных)
        private List<Team> _teams = new List<Team>();
        
        // События
        public event Action<Team> OnTeamCreated;
        public event Action<Team> OnTeamUpdated;
        public event Action<string> OnTeamDeleted;
        
        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
                return;
            }
        }
        
        // Создание новой команды
        public Team CreateTeam(string name, string ownerId)
        {
            var team = new Team
            {
                Id = Guid.NewGuid().ToString(),
                Name = name,
                OwnerId = ownerId,
                Members = new List<TeamMember>
                {
                    new TeamMember
                    {
                        UserId = ownerId,
                        Role = TeamRole.Admin
                    }
                }
            };
            
            _teams.Add(team);
            
            // Если текущий пользователь - владелец, добавляем ID команды в его список
            if (UserManager.Instance.CurrentUser != null && UserManager.Instance.CurrentUser.Id == ownerId)
            {
                UserManager.Instance.CurrentUser.TeamIds.Add(team.Id);
            }
            
            OnTeamCreated?.Invoke(team);
            
            return team;
        }
        
        // Удаление команды
        public bool DeleteTeam(string teamId)
        {
            var team = GetTeamById(teamId);
            
            if (team == null)
                return false;
                
            // Проверяем, имеет ли текущий пользователь право удалять команду
            if (UserManager.Instance.CurrentUser == null || team.OwnerId != UserManager.Instance.CurrentUser.Id)
                return false;
                
            _teams.Remove(team);
            
            // Удаляем ID команды из списка пользователя
            if (UserManager.Instance.CurrentUser.TeamIds.Contains(teamId))
            {
                UserManager.Instance.CurrentUser.TeamIds.Remove(teamId);
            }
            
            OnTeamDeleted?.Invoke(teamId);
            
            return true;
        }
        
        // Добавление участника в команду
        public bool AddMember(string teamId, string userId, TeamRole role)
        {
            var team = GetTeamById(teamId);
            
            if (team == null)
                return false;
                
            // Проверяем, имеет ли текущий пользователь права администратора в команде
            if (!IsUserAdmin(teamId, UserManager.Instance.CurrentUser?.Id))
                return false;
                
            if (team.AddMember(userId, role))
            {
                OnTeamUpdated?.Invoke(team);
                return true;
            }
            
            return false;
        }
        
        // Удаление участника из команды
        public bool RemoveMember(string teamId, string userId)
        {
            var team = GetTeamById(teamId);
            
            if (team == null)
                return false;
                
            // Проверяем, имеет ли текущий пользователь права администратора в команде
            if (!IsUserAdmin(teamId, UserManager.Instance.CurrentUser?.Id))
                return false;
                
            // Нельзя удалить владельца команды
            if (team.OwnerId == userId)
                return false;
                
            if (team.RemoveMember(userId))
            {
                OnTeamUpdated?.Invoke(team);
                return true;
            }
            
            return false;
        }
        
        // Получение команды по ID
        public Team GetTeamById(string teamId)
        {
            return _teams.FirstOrDefault(t => t.Id == teamId);
        }
        
        // Получение списка команд пользователя
        public List<Team> GetUserTeams(string userId)
        {
            return _teams.Where(t => t.Members.Any(m => m.UserId == userId)).ToList();
        }
        
        // Проверка, является ли пользователь администратором команды
        public bool IsUserAdmin(string teamId, string userId)
        {
            if (string.IsNullOrEmpty(userId))
                return false;
                
            var team = GetTeamById(teamId);
            
            if (team == null)
                return false;
                
            // Владелец всегда имеет права администратора
            if (team.OwnerId == userId)
                return true;
                
            var member = team.Members.FirstOrDefault(m => m.UserId == userId);
            
            return member != null && member.Role == TeamRole.Admin;
        }
        
        // Проверка, является ли пользователь участником команды
        public bool IsUserMember(string teamId, string userId)
        {
            if (string.IsNullOrEmpty(userId))
                return false;
                
            var team = GetTeamById(teamId);
            
            if (team == null)
                return false;
                
            return team.Members.Any(m => m.UserId == userId);
        }
    }
} 