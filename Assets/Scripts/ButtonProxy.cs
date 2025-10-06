using UnityEngine;

public class ButtonProxy : MonoBehaviour
{
    public enum ProxyAction { Start, Credit, Option, Quit }
    public ProxyAction action;

    private void Start()
    {
        var button = GetComponent<UnityEngine.UI.Button>();

        if (button != null)
        {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => 
            {
                switch(action)                                    
                {
                    case ProxyAction.Start:
                        GameManager.instance.GameStart();

                        break;
                    case ProxyAction.Credit:
                            

                        break;
                    case ProxyAction.Option:
                        GameManager.instance.GameOption();

                        break;
                    case ProxyAction.Quit:
                        GameManager.instance.GameQuit();

                        break;
                }
            });
        }
    }
}
