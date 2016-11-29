using UnityEngine;
using UnityEngine.UI;

public class PlayerUI : MonoBehaviour {

    public RectTransform fillRect;
    public Image fillImage;
    public Player player;
    public float healthDropOff;
    private Color color;
    private float fill = 1f;
	
	// Update is called once per frame
	void Update () {
        if (player.color != color) {
            color = new Color(player.color.r * 2, player.color.g * 2, player.color.b * 2);
            fillImage.color = color;
        }
        if (fill > (player.currentHealth + 2) / 100f) {
            fill -= healthDropOff;
        } else {
            fill = player.currentHealth / 100f;
        }
        fillRect.localScale = new Vector3(1f, fill, 1f);
    }
}
