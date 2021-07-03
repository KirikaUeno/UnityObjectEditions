using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public float speed = 10;
    public float turnSpeed = 10;
    public float horizontalInput = 0;
    public float verticalInput = 0;
    public float deepInput = 0;
    public float mouseXInput = 0;
    public float mouseYInput = 0;

    // Update is called once per frame
    void Update()
    {
        horizontalInput = Input.GetAxis("Horizontal");
        verticalInput = Input.GetAxis("Vertical");
        deepInput = Input.GetAxis("Deep");
        transform.Translate(Vector3.right * Time.deltaTime * speed * horizontalInput);
        transform.Translate(Vector3.forward * Time.deltaTime * speed * verticalInput);
        transform.Translate(Vector3.up * Time.deltaTime * speed * deepInput);
        if (Input.GetAxis("Fire1") == 1)
        {
            mouseXInput = Input.GetAxis("Mouse X");
            mouseYInput = Input.GetAxis("Mouse Y");

            Vector3 rotateValue = new Vector3(mouseYInput, -mouseXInput, 0);
            transform.eulerAngles -= rotateValue;

            //transform.Rotate(Vector3.up * Time.deltaTime * turnSpeed);
            //transform.Rotate(Vector3.right * Time.deltaTime * turnSpeed);
        }
    }
}
