using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using Orion.Backend.Models;

namespace Orion.Backend.Services
{
    public class OrganizationManager : MonoBehaviour
    {
        public static OrganizationManager Instance { get; private set; }
        
        private List<Organization> _organizations = new List<Organization>();
        private string _orgsFilePath;
        
        // События для UI
        public event Action<List<Organization>> OnOrganizationsUpdated;
        public event Action<Organization> OnOrganizationCreated;
        public event Action<Organization> OnOrganizationUpdated;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                _orgsFilePath = Path.Combine(Application.persistentDataPath, "organizations.json");
                LoadOrganizations();
            }
            else
            {
                Destroy(gameObject);
            }
        }
        
        public async Task<(bool success, string message, Organization createdOrg)> CreateOrganizationAsync(string name, string description, string ownerUserId)
        {
            if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(ownerUserId))
            {
                return (false, "Name and owner are required.", null);
            }

            var owner = UserManager.Instance.GetUserById(ownerUserId);
            if (owner == null)
            {
                return (false, "Owner user not found.", null);
            }

            if (!string.IsNullOrEmpty(owner.OrganizationId))
            {
                return (false, "User already belongs to an organization.", null);
            }

            var newOrg = new Organization(name, description, ownerUserId);
            _organizations.Add(newOrg);
            owner.OrganizationId = newOrg.Id;
            await UserManager.Instance.UpdateUserAsync(owner);
            await SaveOrganizationsAsync();

            OnOrganizationCreated?.Invoke(newOrg);
            OnOrganizationsUpdated?.Invoke(_organizations);

            Debug.Log($"Organization created: {newOrg.Name} by {owner.Username}");
            return (true, "Organization successfully created.", newOrg);
        }
        
        public Organization GetOrganizationById(string id)
        {
            return _organizations.FirstOrDefault(o => o.Id == id);
        }

        public List<Organization> GetOrganizationsByOwner(string ownerUserId)
        {
            return _organizations.Where(o => o.OwnerUserId == ownerUserId).ToList();
        }

        public async Task<bool> AddUserToOrganizationAsync(string organizationId, string userId)
        {
            var org = GetOrganizationById(organizationId);
            if (org == null)
            {
                Debug.LogError($"Organization with ID {organizationId} not found.");
                return false;
            }

            var user = UserManager.Instance.GetUserById(userId);
            if (user == null)
            {
                Debug.LogError($"User with ID {userId} not found.");
                return false;
            }

            if (!string.IsNullOrEmpty(user.OrganizationId))
            {
                Debug.LogError($"User {user.Username} already belongs to an organization.");
                return false;
            }

            if (org.MemberUserIds.Contains(userId))
            {
                Debug.LogError($"User {user.Username} is already a member of this organization.");
                return false;
            }

            org.MemberUserIds.Add(userId);
            user.OrganizationId = org.Id;
            await UserManager.Instance.UpdateUserAsync(user);
            await SaveOrganizationsAsync();

            OnOrganizationUpdated?.Invoke(org);
            OnOrganizationsUpdated?.Invoke(_organizations);

            Debug.Log($"User {user.Username} added to organization {org.Name}");
            return true;
        }
        
        // TODO: Добавить методы для удаления пользователя, обновления данных организации и т.д.

        // --- Сохранение/Загрузка --- 
        private void LoadOrganizations()
        {
            if (File.Exists(_orgsFilePath))
            {
                try
                {
                    string json = File.ReadAllText(_orgsFilePath);
                    _organizations = JsonUtility.FromJson<Serialization<Organization>>(json)?.ToList() ?? new List<Organization>();
                    Debug.Log($"Loaded {_organizations.Count} organizations from {_orgsFilePath}");
                }
                catch (Exception ex)
                {
                    Debug.LogError($"Failed to load organizations from {_orgsFilePath}: {ex.Message}");
                    _organizations = new List<Organization>();
                }
            }
            else
            {
                 _organizations = new List<Organization>();
                 Debug.Log("Organization data file not found. Starting with empty list.");
            }
             OnOrganizationsUpdated?.Invoke(_organizations);
        }

        private async Task SaveOrganizationsAsync()
        {
            try
            {
                string json = JsonUtility.ToJson(new Serialization<Organization>(_organizations), true);
                await Task.Run(() => File.WriteAllText(_orgsFilePath, json));
                Debug.Log($"Saved {_organizations.Count} organizations to {_orgsFilePath}");
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to save organizations to {_orgsFilePath}: {ex.Message}");
            }
        }
    }
} 