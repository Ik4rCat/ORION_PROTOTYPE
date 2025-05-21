using UnityEngine;
using UnityEngine.InputSystem;

public class MenuManager : MonoBehaviour
{

    [SerializeField] GameObject[] regAndLoginMenuParts;
    bool escButton;

    private void Update()
    {
        InputChecker();
    }

    public void InputChecker()
    {
        escButton = Input.GetKeyDown(KeyCode.Escape);
        if (escButton)
        {
            DisableMenuParts();
        }
    }

    void DisableMenuParts()
    {
        for (int i = 0; i < regAndLoginMenuParts.Length; i++)
        {
            if (regAndLoginMenuParts[i].active)
            {
                regAndLoginMenuParts[i].SetActive(false);
                //Debug.Log($"часть меню{i + 1}отключена");
                break;
            }
        }
    }

    //public void ST_Reg_Button()
    //{
    //    regAndLoginMenuParts[0].SetActive(true);
    //}

    public void DEV_Reg_Button()
    {
        regAndLoginMenuParts[0].SetActive(true);
    }

    public void LoginMenu_Button()
    {
        regAndLoginMenuParts[1].SetActive(true);
        regAndLoginMenuParts[0].SetActive(false);
    }

    public void TextButton()
    {
        escButton = true;
        DisableMenuParts(); 

    }

}
