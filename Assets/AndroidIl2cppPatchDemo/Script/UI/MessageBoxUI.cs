using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class MessageBoxUI : MonoBehaviour 
{
	public Text message;
	public Button button;
	public Text buttonText;
    
    public delegate void OnClickDelegate();
    private OnClickDelegate onClick;

    public void Show(string msgStr, string buttenStr, OnClickDelegate onClick)
    {
        this.onClick = onClick;
        Show(msgStr, buttenStr);
    }

    private void Show(string msgStr, string buttenStr)
    {
        Debug.LogError(msgStr);
        message.text = msgStr;
        buttonText.enabled = true;
        buttonText.text = buttenStr;
        gameObject.SetActive(true);
    }

    public void OnClick()
    {
        if (onClick != null)
        {
            onClick();
        }
    }

    public void Close()
    {
        gameObject.SetActive(false);
    }
}
