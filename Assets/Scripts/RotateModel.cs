using UnityEngine;

public class RotateModel : MonoBehaviour
{
    public Transform modelTransform;

    private bool isRotate;
    private Vector3 startPoint;
    private Vector3 startAngel;
    public float rotateScale = 1f;

    void Update()
    {
        if (Input.GetMouseButtonDown(0) && !isRotate)
        {
            isRotate = true;
            startPoint = Input.mousePosition;
            startAngel = modelTransform.eulerAngles;
        }

        if (Input.GetMouseButtonUp(0))
            isRotate = false;

        if (isRotate)
        {
            Vector3 current = Input.mousePosition;
            float x = startPoint.x - current.x;
            modelTransform.eulerAngles = startAngel + new Vector3(0, x * rotateScale, 0);
        }
    }
}