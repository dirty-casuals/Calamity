using UnityEngine;
using System.Collections;

public class CandleFlicker : MonoBehaviour {

    private float minFlickerIntensity = 0.4f;
    private float maxFlickerIntensity = 0.5f;
    private float minRange = 1.0f;
    private float maxRange = 3.0f;
    private float flickerSpeed = 0.1f;
    private float flickerIntensity = 0.0f;
    private float range = 0.0f;
    private bool waiting = false;
    private Light candleLight;
    private float intensityStep = 0.05f;

    private void Start( ) {
        flickerIntensity = Random.Range( minFlickerIntensity, maxFlickerIntensity );
        range = Remap( flickerIntensity, minFlickerIntensity, maxFlickerIntensity, minRange, maxRange );
        candleLight = GetComponent<Light>( );
        maxRange = candleLight.range;
        maxFlickerIntensity = candleLight.intensity;
        minFlickerIntensity = maxFlickerIntensity - 0.15f;
    }

    // Update is called once per frame
    private void Update( ) {
        if (!waiting) {
            StartCoroutine( DoFlicker( ) );
        }
    }

    private IEnumerator DoFlicker( ) {
        waiting = true;
        float currentIntensity = candleLight.intensity;

        if (candleLight.intensity > flickerIntensity) {
            while (candleLight.intensity > flickerIntensity) {
                candleLight.intensity = candleLight.intensity - intensityStep;
                yield return new WaitForSeconds( flickerSpeed );
            }
        } else if (candleLight.intensity < flickerIntensity) {
            while (candleLight.intensity < flickerIntensity) {
                candleLight.intensity = candleLight.intensity + intensityStep;
                yield return new WaitForSeconds( flickerSpeed );
            }
        }
        
        candleLight.range = range;

        flickerIntensity = Random.Range( minFlickerIntensity, maxFlickerIntensity );
        range = Remap( flickerIntensity, minFlickerIntensity, maxFlickerIntensity, minRange, maxRange );
        yield return new WaitForSeconds( flickerSpeed );
        waiting = false;
    }

    private float Remap( float val, float low1, float high1, float low2, float high2 ) {
        return low2 + (val - low1) * (high2 - low2) / (high1 - low1);
    }
}