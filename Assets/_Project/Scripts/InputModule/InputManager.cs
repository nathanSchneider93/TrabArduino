﻿using System;
using System.IO.Ports;
using UnityEngine;

namespace SpaceshipWarrior.InputModule
{
    public class InputManager : MonoBehaviour
    {
        public delegate void KeyPressedHandler();

        public delegate void LookPointChangedHandler(Vector3 value);

        public delegate void VerticalRotationDeltaChangedHandler(float value);

        public event KeyPressedHandler OnFireKeyPressed;
        public event LookPointChangedHandler OnLookPointChanged;
        public event VerticalRotationDeltaChangedHandler OnVerticalRotationDeltaChanged;

        private const int BaudRateDefaultValue = 9600;
        private const float VerticalAngleFactorDefaultValue = 45f;
        private const float NormalizerFactor = 1.0f / 32768.0f;
        private const string PortNameDefaultValue = "COM4";

        [Header("Arduino")]
        [SerializeField] private string _portName = PortNameDefaultValue;
        [SerializeField] private int _baudRate = BaudRateDefaultValue;
        [SerializeField] private float _verticalAngleFactor = VerticalAngleFactorDefaultValue;

        [Header("Keyboard and Mouse")]
        [SerializeField] private KeyCode _fireKey;

        private Action _onUpdate;
        private SerialPort _serialPort;

        public void Initialize()
        {
            var foundPort = false;
            string[] availablePorts = SerialPort.GetPortNames();

            foreach (string portName in availablePorts)
            {
                if (!string.Equals(portName, _portName))
                {
                    continue;
                }

                foundPort = true;

                break;
            }

            if (!foundPort)
            {
                _onUpdate = OnUpdateKeyboardAndMouse;

                Debug.LogError($"No port with name {_portName} was found.");

                return;
            }

            _serialPort = new SerialPort(_portName, _baudRate) { ReadTimeout = 2000 };
            _serialPort.Open();
            _onUpdate = OnUpdateArduino;
        }

        public void OnUpdate()
        {
            _onUpdate?.Invoke();
        }

        private void OnUpdateKeyboardAndMouse()
        {
            OnLookPointChanged?.Invoke(Input.mousePosition);

            if (Input.GetKeyDown(_fireKey))
            {
                OnFireKeyPressed?.Invoke();
            }
        }

        private void OnUpdateArduino()
        {
            var data = "";

            if (_serialPort.IsOpen)
            {
                data = _serialPort.ReadLine();
            }

            if (string.IsNullOrWhiteSpace(data))
            {
                return;
            }

            const float tolerance = 0.025f;
            const char splitter = ';';

            string[] rawData = data.Split(splitter);
            float gyroscopeVerticalAngle = int.Parse(rawData[4]) * NormalizerFactor;

            print($"{nameof(rawData)}: {rawData[4]}\n{nameof(gyroscopeVerticalAngle)}: {gyroscopeVerticalAngle}");

            if (Mathf.Abs(gyroscopeVerticalAngle) < tolerance)
            {
                gyroscopeVerticalAngle = 0f;
            }

            OnVerticalRotationDeltaChanged?.Invoke(-gyroscopeVerticalAngle * _verticalAngleFactor);
        }
    }
}
