using UnityEngine;
using TMPro;
public class SliderController : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI sliderText = null;
    [SerializeField] private float maxSliderAmount = 100.0f;

    [SerializeField] private bool integer;
    [SerializeField] private bool multiply;

    public void sliderChange(float value)
    {
        if (integer)
        {
            sliderText.text = value.ToString("0");
        }
        if (multiply)
        {
            float localValue = value * maxSliderAmount;
            sliderText.text = localValue.ToString("0.00");
        }
        else
        {
            sliderText.text = value.ToString("0.00");
        }
        
    }
}
