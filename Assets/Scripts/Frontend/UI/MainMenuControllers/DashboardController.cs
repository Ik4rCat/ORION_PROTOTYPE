using UnityEngine;
using TMPro;
using Orion.Backend.Services;
namespace Orion.Frontend.UI
{
    public class DashboardController : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI welcomeText, projectsCountText,
            tasksCountText, teamMembersCountText;

        private void OnEnable()
        {
            RefreshDashboard();
        }

        public void RefreshDashboard()
        {
            var user = UserManager.Instance.CurrentUser;
            if (user == null) return;

            welcomeText.text = $"Здравствуйте, {user.Username}!";

            // Получаем статистику из менеджеров
            var orgId = user.OrganizationId;
            if (string.IsNullOrEmpty(orgId)) return;

            var projects = ProjectManager.Instance.GetProjectsByOrganization(orgId);
            projectsCountText.text = projects.Count.ToString();

            var tasks = TaskManager.Instance.GetTasksByOrganization(orgId);
            tasksCountText.text = tasks.Count.ToString();

            var members = UserManager.Instance.GetUsersByOrganization(orgId);
            teamMembersCountText.text = members.Count.ToString();
        }

    }
}
