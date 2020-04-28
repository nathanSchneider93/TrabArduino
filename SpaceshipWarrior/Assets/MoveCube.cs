using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO.Ports;


public class MoveCube : MonoBehaviour 
{
public float speed;
public float amountToMove;
SerialPort sp = new SerialPort("COM3",9600);


	// Use this for initialization
	void Start ()
    {
        sp.Open();
        sp.ReadTimeout = 1;
		
	}
	
	// Update is called once per frame
	void Update () 
	{
        amountToMove = speed * Time.deltaTime;

        if (sp.IsOpen)
        {
            try
            {
                MoveObject(sp.ReadByte());
                print(sp.ReadByte());
            }
            catch (System.Exception)
            {
            }
        }        
	}

	void MoveObject(int direction)
	{
		if(direction == 1)
		{
			transform.Translate(Vector3.left * amountToMove , Space.World);
		}
		if(direction == 2)
		{
            transform.Translate(Vector3.right * amountToMove , Space.World);
		}

	}
}
