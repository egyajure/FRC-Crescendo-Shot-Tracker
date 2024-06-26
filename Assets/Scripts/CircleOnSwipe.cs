using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class CircleOnSwipe : MonoBehaviour
{
    public static CircleOnSwipe instance;
    public delegate void Swipe(Vector2 direction);

    public event Swipe SwipePerformed;

    [SerializeField] private InputAction position, press;
    [SerializeField] private float swipeResistanceX = 100;
    [SerializeField] private float swipeResistanceY = 200;


    private Vector2 initialPos;
    private Vector2 currentPos => position.ReadValue<Vector2>();
    public GameObject clone, otherclone;
    private bool isRunning = true;

    public LoadGrid grid_manager;

    void Start()
    {
        position.Enable();
        press.Enable();
    }

    void Update()
    {
        if (!isRunning) return;
        press.performed += _ => { initialPos = currentPos; };
        press.canceled += _ => DetectSwipe();
        instance = this;
    }

    private void OnDestroy()
    {
        position.Disable(); // Make sure to disable the input actions when the script is destroyed
        press.Disable();
    }

    private void DetectSwipe()
    {
        Vector2 delta = currentPos - initialPos;
        Vector2 direction = Vector2.zero;

        if (Mathf.Abs(delta.y) > swipeResistanceY)
        {
            //we have swiped if we got here
            direction.y = Mathf.Clamp(delta.y, -1, 1);
            switch_scene(direction);
            isRunning = false;
            return;
        }
        else if (Mathf.Abs(delta.x) > swipeResistanceX)
        {
            //we have swiped if we got here
            if (delta.x > 0)
            {
                Debug.Log("updating hits");
                grid_manager.updateGrid(initialPos, true);
            }
            else
            {
                Debug.Log("updating misses");
                grid_manager.updateGrid(initialPos, false);
            }

            direction.x = Mathf.Clamp(delta.x, -1, 1);
            make_sprite(direction, initialPos);
        }
        if (direction != Vector2.zero & SwipePerformed != null)
        {
            SwipePerformed(direction);
        }
    }

    private void make_sprite(Vector2 direction, Vector2 position)
    {
        GameObject objToInstantiate = null;
        if (direction.x > 0)
        {
            objToInstantiate = clone;
        }
        else if (direction.x < 0)
        {
            objToInstantiate = otherclone;
        }

        if (objToInstantiate != null)
        {
            GameObject obj = Instantiate(objToInstantiate, Camera.main.ScreenToWorldPoint(position) + Vector3.forward * 10, Quaternion.identity);
            // DontDestroyOnLoad(obj); // Ensure the instantiated object persists across scene changes
        }
    }

    private void switch_scene(Vector2 direction)
    {
        if (direction.y < 0)
        {
            SceneManager.LoadScene("Home");
            return;
        }
        if (direction.y > 0)
        {
            SceneManager.LoadScene("HeatMapScene");
            return;

        }
    }
}
