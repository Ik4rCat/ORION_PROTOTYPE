using UnityEngine;
using UnityEngine.UI;
using Orion.Backend.Services;

namespace Orion.Frontend.UI
{
    public class MAINmenuManager : MonoBehaviour
    {
        [SerializeField] private GameObject dashboardPanel, projectsPanel, tasksPanel, orgPanel, devToolsPanel;


        public void ShowDashboard() => ActivatePanel(dashboardPanel);
        public void ShowProjects() => ActivatePanel(projectsPanel);
        public void ShowTasks() => ActivatePanel(tasksPanel);
        public void ShowOrganization() => ActivatePanel(orgPanel);
        public void ShowDevTools() => ActivatePanel(devToolsPanel);

        private void ActivatePanel(GameObject panel)
        {
            dashboardPanel.SetActive(panel == dashboardPanel);
            projectsPanel.SetActive(panel == projectsPanel);
            tasksPanel.SetActive(panel == tasksPanel);
            orgPanel.SetActive(panel == orgPanel);
            devToolsPanel.SetActive(panel == devToolsPanel);
        }
    }
}
