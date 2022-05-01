using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class DestructionToggle : MonoBehaviour //Button used to control the destruction mode of the CA Click drawerer
{
    [SerializeField] private CAClickDrawer ClickDrawer;
    [SerializeField] private Image image;
    bool is_destructive = false;

    // Start is called before the first frame update
    void Start()
    {
        ClickDrawer.setIsDestructive(is_destructive);
        updateColour();
    }

    private void updateColour() {
        image.color = is_destructive ? new Color(0.8773585f,0.3448735f,0.3448735f,0.8f) : new Color(0.3732461f,0.8584906f,0.3698528f,0.8f);
    }

    public void onSelected(){
        is_destructive = !is_destructive;
        ClickDrawer.setIsDestructive(is_destructive);
        updateColour();
    }
}
