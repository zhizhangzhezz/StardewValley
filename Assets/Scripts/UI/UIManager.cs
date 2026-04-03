using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : SingletonMonobehaviour<UIManager>
{
    private bool _pauseMenuOn = false;//暂停菜单是否开启
    [SerializeField] private UIInventoryBar uiInventoryBar = null;
    [SerializeField] private PauseMenuInventoryManagement pauseMenuInventoryManagement = null;
    [SerializeField] private GameObject pauseMenu = null;
    [SerializeField] private GameObject[] menuTabs = null;
    [SerializeField] private Button[] menuButtons = null;

    public bool PauseMenuOn { get => _pauseMenuOn; set => _pauseMenuOn = value; }

    protected override void Awake()
    {
        base.Awake();

        pauseMenu.SetActive(false);
    }

    private void Update()
    {
        PauseMenu();
    }

    private void PauseMenu()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (_pauseMenuOn)
            {
                DisableMenu();
            }
            else
            {
                EnableMenu();
            }
        }
    }

    private void EnableMenu()
    {
        //取消选中所有在拖动和选择的物品
        uiInventoryBar.DestroyCurrentDraggedItem();
        uiInventoryBar.ClearCurrentSelectedItem();

        PauseMenuOn = true;
        Player.Instance.PlayerInputIsDisabled = true;
        Time.timeScale = 0f;//暂停游戏时间
        pauseMenu.SetActive(true);
        // Collect garbage
        System.GC.Collect();

        HighlightButtonForSelectedTab();//设置按钮高亮
    }

    private void DisableMenu()
    {
        pauseMenuInventoryManagement.DestroyCurrentDraggedItem();

        PauseMenuOn = false;
        Player.Instance.PlayerInputIsDisabled = false;
        Time.timeScale = 1f;//恢复游戏时间
        pauseMenu.SetActive(false);
    }

    private void HighlightButtonForSelectedTab()
    {
        for (int i = 0; i < menuTabs.Length; i++)
        {
            if (menuTabs[i].activeSelf)
            {
                SetButtonColorToActive(menuButtons[i]);
            }
            else
            {
                SetButtonColorToInactive(menuButtons[i]);
            }
        }
    }

    private void SetButtonColorToActive(Button button)
    {
        ColorBlock colors = button.colors;
        colors.normalColor = colors.pressedColor;
        button.colors = colors;
    }

    private void SetButtonColorToInactive(Button button)
    {
        ColorBlock colors = button.colors;
        colors.normalColor = colors.disabledColor;
        button.colors = colors;
    }

    public void SwitchPauseMenuTab(int tabNum)//OnClick调用
    {
        for (int i = 0; i < menuTabs.Length; i++)
        {
            if (i != tabNum)
            {
                menuTabs[i].SetActive(false);
            }
            else
            {
                menuTabs[i].SetActive(true);
            }
        }

        HighlightButtonForSelectedTab();
    }

}
