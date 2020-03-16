using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace gPearlAnalyzer.Model
{
    public class MotorControl
    {
        public List<StageMotor> serialPorts = new List<StageMotor>();
        public bool _driveConnected = false;

        public bool Connect(int numRetries = 1)
        {
            bool foundRotation = false;
            bool foundLinear = false;

            bool RotationConnected = false;
            bool LinearConnected = false;

            for (int i = 0; i < numRetries; i++)
            {
                SerialPort rotationSerialPort = null;
                SerialPort linearSerialPort = null;
                try
                {
                    App.LogEntry.AddEntry("Trying to connect to Stepper Motor");

                    string[] ports = SerialPort.GetPortNames();

                    foreach (string port in ports)
                    {
                        try
                        {
                            if (!foundRotation)
                            {
                                rotationSerialPort = new System.IO.Ports.SerialPort(port, 9600, System.IO.Ports.Parity.None, 8, System.IO.Ports.StopBits.One);
                                rotationSerialPort.DtrEnable = true;
                                rotationSerialPort.NewLine = "\r";
                                rotationSerialPort.Encoding = System.Text.Encoding.ASCII;
                                rotationSerialPort.ReadTimeout = 2000;
                                rotationSerialPort.WriteTimeout = 2000;

                                foundRotation = ConnectSerialPort(rotationSerialPort);
                                if (!foundRotation)
                                {
                                    rotationSerialPort.Close();
                                } else
                                {
                                    Console.WriteLine(port + " found" + " 9600");
                                    continue;
                                }
                            }
                            if (!foundLinear)
                            {
                                linearSerialPort = new System.IO.Ports.SerialPort(port, 19200, System.IO.Ports.Parity.None, 8, System.IO.Ports.StopBits.One);
                                linearSerialPort.DtrEnable = true;
                                linearSerialPort.NewLine = "\r";
                                linearSerialPort.Encoding = System.Text.Encoding.ASCII;
                                linearSerialPort.ReadTimeout = 2000;
                                linearSerialPort.WriteTimeout = 2000;

                                foundLinear = ConnectSerialPort(linearSerialPort, false);
                                if (!foundLinear)
                                {
                                    linearSerialPort.Close();
                                } else
                                {
                                    Console.WriteLine(port + " found" + " 19200");
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine(ex.ToString());
                            
                            if (!foundRotation)
                            {
                                if (rotationSerialPort != null)
                                    rotationSerialPort.Close();
                            }

                            if (!foundLinear)
                            {
                                if (linearSerialPort != null)
                                    linearSerialPort.Close();
                            }
                        }
                    }

                    if(foundRotation && foundLinear)
                    {
                        RotationConnected = SetSerialPort(rotationSerialPort, MotorType.Rotation);

                        LinearConnected = SetSerialPort(linearSerialPort, MotorType.Linear);

                        if(RotationConnected && LinearConnected)
                        {
                            App.LogEntry.AddEntry("Connected to Stepper Motor");
                            _driveConnected = true;
                            serialPorts.Add(new StageMotor(rotationSerialPort, MotorType.Rotation));
                            serialPorts.Add(new StageMotor(linearSerialPort, MotorType.Linear));

                            return _driveConnected;
                        } else
                        {
                            if (linearSerialPort != null)
                                linearSerialPort.Close();

                            if (rotationSerialPort != null)
                                rotationSerialPort.Close();

                            App.LogEntry.AddEntry("Could not connect to Stepper Motor", true);
                            _driveConnected = false;
                            return _driveConnected;
                        }
                    } else
                    {
                        if (linearSerialPort != null)
                            linearSerialPort.Close();

                        if (rotationSerialPort != null)
                            rotationSerialPort.Close();

                        App.LogEntry.AddEntry("Could not connect to Stepper Motor", true);
                        _driveConnected = false;
                        return _driveConnected;
                    }

                }
                catch (Exception ex)
                {
                    if (linearSerialPort != null)
                        linearSerialPort.Close();
                    if (rotationSerialPort != null)
                        rotationSerialPort.Close();

                    App.LogEntry.AddEntry("Could not connect to Stepper Motor : " + ex.Message);

                    _driveConnected = false;
                    System.Threading.Thread.Sleep(500);
                }

            }

            return _driveConnected;
        }

        public bool DisConnect()
        {
            bool result = false;

            if (serialPorts.Count > 0)
            {
                Homing();
                MoveUpDown(false);
                Thread.Sleep(500);
            }

            for(int i = 0; i < serialPorts.Count; i++)
            {
                //serialPorts[i].DisableMotor();
                serialPorts[i].Close();
            }
            serialPorts.Clear();
            return result;
        }

        public bool LightOnOff(StageMotor motor)
        {
            bool result = false;

            return result;
        }

        public bool RotateStage(bool enable, MotorType type = MotorType.Rotation)
        {
            bool result = false;
            if (!enable)
            {
                serialPorts[(int)type].StopMotor();
                return true;
            }
            else
            {
                try
                {
                    serialPorts[(int)type].ContinuousMotion(Direction.CW);
                    result = true;
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Failed to rotate the stage: " + ex.Message);
                    result = false;
                }
                return result;
            }
        }

        public bool MoveUpDown(bool up, MotorType type = MotorType.Linear)
        {
            bool result = false;
            try
            {
                serialPorts[(int)type].Move(up ? Direction.CCW : Direction.CW, 100);
                result = true;
            }catch(Exception ex)
            {
                Console.WriteLine("Failed to move up/down: " + ex.Message);
                result = false;
            }
            return result;
        }

        public bool Homing(MotorType type = MotorType.Linear)
        {
            bool result = false;
            try
            {
                serialPorts[(int)type].Homing();
                result = true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Failed to homing: " + ex.Message);
                result = false;
            }
            return result;
        }

        bool SetSerialPort(SerialPort serialPort, MotorType motorType)
        {
            bool result = false;
            try
            {
                //make sure in SCL mode
                serialPort.WriteLine("PM");
                if (serialPort.ReadLine().Substring(3) != "2")
                {
                    serialPort.WriteLine("PM2");
                    //CheckAck();
                    App.LogEntry.AddEntry("ST5 : Please power cycle the Motor Drive and try connecting again", true);
                    return false;
                }

                serialPort.WriteLine("PR5");
                if (!CheckAck(serialPort))
                {
                    return false;
                }
                serialPort.WriteLine("SKD"); //Stop and Kill 
                if (!CheckAck(serialPort))
                {
                    return false;
                }
                serialPort.WriteLine("MV");
                string mvRead = serialPort.ReadLine();
                try
                {
                    SERVO_MOTOR_MODEL _model = (SERVO_MOTOR_MODEL)Convert.ToInt32(mvRead.Substring(4, 3)); ;
                }
                catch (Exception /*ex*/)
                {
                    App.LogEntry.AddEntry("Unsupported Motor Drive", true);
                    return false;
                }
                App.LogEntry.AddEntry("ST5 Model/Revision: " + mvRead);
                serialPort.WriteLine("OP");
                App.LogEntry.AddEntry("ST5 Option Board: " + serialPort.ReadLine());

                serialPort.WriteLine("SC");
                string response = serialPort.ReadLine().Substring(3);
                App.LogEntry.AddEntry("ST5 Status: 0x" + response);
                bool _motorEnabled = (Convert.ToInt32(response.Substring(3, 1)) % 2) == 1;
                bool _driveBusy = (Convert.ToInt32(response.Substring(2, 1)) % 2) != 0;

                serialPort.WriteLine("AC100");
                if (!CheckAck(serialPort))
                {
                    return false;
                }
                serialPort.WriteLine("DC100");
                if (!CheckAck(serialPort))
                {
                    return false;
                }
                result = true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Failed in SetSerialPort: " + motorType.ToString() + " exception: " + ex.Message);
                result = false;
            }

            return result;
        }

        bool ConnectSerialPort(SerialPort serialPort, bool rotation = true)
        {
            bool result = false;
            try
            {
                if (!serialPort.IsOpen)
                    serialPort.Open();

                serialPort.DiscardInBuffer();
                serialPort.DiscardOutBuffer();

                //write some nulls
                serialPort.Write("\0");
                serialPort.Write("\0");
                serialPort.Write("\0");
                serialPort.DiscardInBuffer();
                serialPort.DiscardOutBuffer();

                serialPort.WriteLine("MV");
                System.Threading.Thread.Sleep(100);
                serialPort.DiscardInBuffer();

                serialPort.WriteLine("BR");//baud rate 1 = 9600
                string response = serialPort.ReadLine().Substring(3);

                if (rotation)
                {
                    if (response == "1")
                    {
                        result = true;
                    }
                } else
                {
                    if (response == "2")
                    {
                        result = true;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Failed in connecting to serial port: " + ex.Message);
                result = false;
            }
            return result;
        }

        bool CheckAck(SerialPort serialPort)
        {
            var response = serialPort.ReadLine();
            if (!response.Equals("%") && !response.Equals("*"))
            {
                //throw new ApplicationException("The command was not acknowledged - please try again momentarily.");
                Console.WriteLine("The command was not acknowledged");
                return false;
            }
            return true;
        }
    }
}
