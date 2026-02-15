using UnityEngine;

public class SpinningBlade : MonoBehaviour
{
    private float rotationFactor = 1;

    private void Update()
    {
        transform.Rotate(new Vector3(0, 0, 1), 360 * rotationFactor * Time.deltaTime);
    }

    public void StopRotation() => rotationFactor = 0;
}
