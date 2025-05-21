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
using Orion.Backend.Models;

namespace Orion.Services
{
    public class KanbanManager : MonoBehaviour
    {
        public static KanbanManager Instance { get; private set; }
        
        // Список досок (в реальном приложении будет загружаться из базы данных)
        private List<KanbanBoard> _boards = new List<KanbanBoard>();
        
        // События
        public event Action<KanbanBoard> OnBoardCreated;
        public event Action<KanbanBoard> OnBoardUpdated;
        public event Action<string> OnBoardDeleted;
        public event Action<KanbanCard, string, string> OnCardMoved;
        
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
        
        // Создание новой доски
        public KanbanBoard CreateBoard(string title, string teamId = null)
        {
            var board = new KanbanBoard
            {
                Id = Guid.NewGuid().ToString(),
                Title = title
            };
            
            // Добавление стандартных колонок
            board.AddColumn("К выполнению");
            board.AddColumn("В процессе");
            board.AddColumn("Готово");
            
            _boards.Add(board);
            
            // Если указан ID команды, добавляем доску к команде
            if (!string.IsNullOrEmpty(teamId))
            {
                var team = TeamManager.Instance.GetTeamById(teamId);
                if (team != null)
                {
                    team.BoardIds.Add(board.Id);
                }
            }
            
            OnBoardCreated?.Invoke(board);
            
            return board;
        }
        
        // Удаление доски
        public bool DeleteBoard(string boardId)
        {
            var board = GetBoardById(boardId);
            
            if (board == null)
                return false;
                
            _boards.Remove(board);
            
            // Если доска привязана к команде, удаляем ID доски из списка команды
            if (UserManager.Instance.CurrentUser != null)
            {
                var userTeams = TeamManager.Instance.GetUserTeams(UserManager.Instance.CurrentUser.Id);
                foreach (var team in userTeams)
                {
                    if (team.BoardIds.Contains(boardId))
                    {
                        team.BoardIds.Remove(boardId);
                    }
                }
            }
            
            OnBoardDeleted?.Invoke(boardId);
            
            return true;
        }
        
        // Создание новой карточки
        public KanbanCard CreateCard(string boardId, string columnId, string title, string description = "")
        {
            var board = GetBoardById(boardId);
            
            if (board == null)
                return null;
                
            var column = board.Columns.FirstOrDefault(c => c.Id == columnId);
            
            if (column == null)
                return null;
                
            var card = column.AddCard(title, description);
            
            OnBoardUpdated?.Invoke(board);
            
            return card;
        }
        
        // Перемещение карточки
        public bool MoveCard(string boardId, string cardId, string sourceColumnId, string targetColumnId, int targetIndex = -1)
        {
            var board = GetBoardById(boardId);
            
            if (board == null)
                return false;
                
            if (board.MoveCard(cardId, sourceColumnId, targetColumnId, targetIndex))
            {
                OnBoardUpdated?.Invoke(board);
                OnCardMoved?.Invoke(
                    board.Columns.FirstOrDefault(c => c.Id == targetColumnId)?.Cards.FirstOrDefault(card => card.Id == cardId),
                    sourceColumnId,
                    targetColumnId
                );
                return true;
            }
            
            return false;
        }
        
        // Добавление колонки
        public bool AddColumn(string boardId, string title)
        {
            var board = GetBoardById(boardId);
            
            if (board == null)
                return false;
                
            board.AddColumn(title);
            OnBoardUpdated?.Invoke(board);
            
            return true;
        }
        
        // Удаление колонки
        public bool RemoveColumn(string boardId, string columnId)
        {
            var board = GetBoardById(boardId);
            
            if (board == null)
                return false;
                
            var result = board.RemoveColumn(columnId);
            
            if (result)
                OnBoardUpdated?.Invoke(board);
                
            return result;
        }
        
        // Получение доски по ID
        public KanbanBoard GetBoardById(string boardId)
        {
            return _boards.FirstOrDefault(b => b.Id == boardId);
        }
        
        // Получение списка досок команды
        public List<KanbanBoard> GetTeamBoards(string teamId)
        {
            var team = TeamManager.Instance.GetTeamById(teamId);
            
            if (team == null)
                return new List<KanbanBoard>();
                
            return _boards.Where(b => team.BoardIds.Contains(b.Id)).ToList();
        }
        
        // Получение списка всех досок, доступных пользователю
        public List<KanbanBoard> GetUserBoards(string userId)
        {
            var userTeams = TeamManager.Instance.GetUserTeams(userId);
            var boardIds = new HashSet<string>();
            
            foreach (var team in userTeams)
            {
                foreach (var boardId in team.BoardIds)
                {
                    boardIds.Add(boardId);
                }
            }
            
            return _boards.Where(b => boardIds.Contains(b.Id)).ToList();
        }
    }
} 