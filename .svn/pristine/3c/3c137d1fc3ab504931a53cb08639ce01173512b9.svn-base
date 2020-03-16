using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using Timer = System.Timers.Timer;

namespace gPearlAnalyzer.Model
{
    public enum MotorType
    {
        Rotation = 0,
        Linear    
    }

    public enum Direction
    {
        CW = 0,
        CCW
    };

    public enum SERVO_MOTOR_MODEL
    {
        ST5Q = 22,
        ST5Plus = 26
    };

    public class StageMotor
    {
        Timer _statusTimer = null;
        SerialPort _serialPort = null;
        bool _motorEnabled = false;
        int _stepsPerRevolution = 63500;
        double _continuousVelocity;
        bool _driveBusy = false;
        bool _driveConnected = false;
        Object _statusLock = new Object();
        SERVO_MOTOR_MODEL _model = SERVO_MOTOR_MODEL.ST5Q;
        MotorType _motortype;


        public event EventHandler DriveNotReady;
        public event EventHandler DriveReady;

        public StageMotor(SerialPort serialPort, MotorType motorType)
        {
            _serialPort = serialPort;
            _motortype = motorType;

            if (motorType == MotorType.Rotation)
            {
                ContinuousVelocity = App.Settings.MotorContinuousVelocity;
            } else
            {
                CheckLimitHoming();
            }

            serialPort.DiscardInBuffer();
            serialPort.DiscardOutBuffer();

            ResetPosition();

            _statusTimer = new Timer();
            _statusTimer.Interval = 50;
            _statusTimer.AutoReset = false;
            _statusTimer.Elapsed += new ElapsedEventHandler(_statusTimer_Elapsed);
        }

        public void Close()
        {
            _serialPort.DiscardInBuffer();
            _serialPort.DiscardOutBuffer();
            _serialPort.Dispose();
            _serialPort.Close();
        }

        public int StepsPerRev
        {
            get
            {
                return _stepsPerRevolution;
            }
            set
            {
                _stepsPerRevolution = value;
            }
        }

        void CheckAck()
        {
            var response = _serialPort.ReadLine();
            if (!response.Equals("%") && !response.Equals("*"))
            {
                throw new ApplicationException("The command was not acknowledged - please try again momentarily.");
            }
        }

        public void ResetPosition()
        {
            //Apparently we have no encoder so this command is not necessary
            //if (_model == SERVO_MOTOR_MODEL.ST5Q)
            //{
            //    _serialPort.WriteLine("EP0");
            //    CheckAck();
            //}

            _serialPort.WriteLine("SP0");
            CheckAck();
        }

        public bool LightOn(bool on, int output = 1)
        {
            if (!_motorEnabled)
                EnableMotor();

            bool result = false;
            string s = on ? "IL" : "IH";
            _serialPort.WriteLine(s + output.ToString());

            try
            {
                CheckAck();
                result = true;
            }
            catch(Exception ex)
            {
                result = false;
            }
            return result;
        }

        public bool isHoming = false;
        public bool isLimit = false;

        public void CheckLimitHoming()
        {
            try
            {
                _serialPort.WriteLine("IS");
                string response = _serialPort.ReadLine();
                if (Int32.Parse(response.Substring(response.Length - 1)) == 1)
                {
                    isHoming = true;
                } else
                {
                    isHoming = false;
                }

                if (Int32.Parse(response.Substring(response.Length - 2)) == 1)
                {
                    isLimit = true;
                }
                else
                {
                    isLimit = false;
                }
            } catch(Exception ex)
            {
                Console.WriteLine("failed to check homimg and limit status");
            }
        }

        public void Homing()
        {
            if (!_motorEnabled)
                EnableMotor();

            try
            {
                _serialPort.WriteLine("IS");
                string response = _serialPort.ReadLine();
                if(Int32.Parse(response.Substring(response.Length - 1, 1)) == 1)
                {
                    isHoming = true;
                    Console.WriteLine("Already in homing position.");
                    return;
                }

                _serialPort.WriteLine("DI");
                response = _serialPort.ReadLine();
                if (Int32.Parse(response.Substring(3)) > 0)
                {
                    _serialPort.WriteLine("DI-200");
                    CheckAck();
                }

                _serialPort.WriteLine("SH1H");
                CheckAck();
                //wait for motor to start
                do
                {
                    _serialPort.WriteLine("SC");
                    response = _serialPort.ReadLine();
                    if (response.Length == 7)
                    {
                        response = response.Substring(3);
                        _driveBusy = (Convert.ToInt32(response.Substring(2, 1)) % 2) != 0;
                    }
                } while (!_driveBusy);

                // wait for homing position
                while (!isHoming)
                {
                    Thread.Sleep(300);
                    CheckLimitHoming();
                }
                OnDriveNotReady(null);//signal motor started
            }
            catch (Exception ex)
            {
                App.LogEntry.AddEntry("ST5 RotateByAngle Failed : " + ex.Message);
                return;
            }

            _statusTimer.Start();
        }

        public void Move(Direction dir, int steps)
        {
            int newPosition = steps;

            if (!_motorEnabled)
                EnableMotor();

            if (dir == Direction.CCW)
                newPosition *= -1;

            try
            {
                _serialPort.WriteLine("FL" + Convert.ToString(newPosition));
                CheckAck();
                //wait for motor to start
                do
                {
                    _serialPort.WriteLine("SC");
                    string response = _serialPort.ReadLine();
                    if (response.Length == 7)
                    {
                        response = response.Substring(3);
                        _driveBusy = (Convert.ToInt32(response.Substring(2, 1)) % 2) != 0;
                    }
                } while (!_driveBusy);
                OnDriveNotReady(null);//signal motor started
            }
            catch (Exception ex)
            {
                App.LogEntry.AddEntry("ST5 RotateByAngle Failed : " + ex.Message);
                return;
            }

            _statusTimer.Start();

        }


        public void RotateByAngle(int noOfDegrees, Direction _direction)
        {
            int newPosition = (_stepsPerRevolution * noOfDegrees) / 360;

            if (!_motorEnabled)
                EnableMotor();

            if (_direction == Direction.CCW)
                newPosition *= -1;

            try
            {
                _serialPort.WriteLine("FL" + Convert.ToString(newPosition));
                CheckAck();
                //wait for motor to start
                do
                {
                    _serialPort.WriteLine("SC");
                    string response = _serialPort.ReadLine();
                    if (response.Length == 7)
                    {
                        response = response.Substring(3);
                        _driveBusy = (Convert.ToInt32(response.Substring(2, 1)) % 2) != 0;
                    }
                } while (!_driveBusy);
                OnDriveNotReady(null);//signal motor started
            }
            catch (Exception ex)
            {
                App.LogEntry.AddEntry("ST5 RotateByAngle Failed : " + ex.Message);
                return;
            }

            _statusTimer.Start();
        }

        public void ContinuousMotion(Direction _direction)
        {
            int dir = 1;

            if (!_motorEnabled)
                EnableMotor();

            if (_direction == Direction.CCW)
                dir = -1;

            try
            {
                _serialPort.WriteLine("DI" + dir.ToString());
                CheckAck();
                _serialPort.WriteLine("JS" + ContinuousVelocity.ToString());
                CheckAck();
                _serialPort.WriteLine("CJ");
                CheckAck();
            }
            catch (Exception ex)
            {
                App.LogEntry.AddEntry("ST5 ContinuousMotion Failed : " + ex.Message);
                return;
            }

            _statusTimer.Start();
        }

        public void StopMotor()
        {
            try
            {
                lock (_statusLock)
                {
                    _serialPort.WriteLine("SKD");
                    CheckAck();
                }
            }
            catch (Exception ex)
            {
                App.LogEntry.AddEntry("ST5 StopMotor : " + ex.Message);
            }
        }

        public void DisableMotor()
        {
            _serialPort.WriteLine("MD");
            try
            {
                CheckAck();
            }
            catch
            {
                throw;
            }
            _motorEnabled = false;
        }

        public void EnableMotor()
        {
            _serialPort.WriteLine("ME");
            try
            {
                CheckAck();
            }
            catch
            {
                throw;
            }
            _motorEnabled = true;
        }

        public bool DriveBusy
        {
            get
            {
                return _driveBusy;
            }
        }

        public bool MotorConnected
        {
            get
            {
                return _driveConnected;
            }
        }

        public double ContinuousVelocity
        {
            get
            {
                _serialPort.WriteLine("VE");
                string response = _serialPort.ReadLine();
                if (response.Length > 3)
                    _continuousVelocity = Convert.ToDouble(response.Substring(3));
                else
                    _continuousVelocity = -1;
                return (_continuousVelocity);
            }

            set
            {
                if (Math.Abs(value) > 3)
                {
                    throw new ApplicationException("Continuous velocity must not exceed 3 revolutions per second");
                }

                _serialPort.WriteLine("VE" + value.ToString());
                try
                {
                    CheckAck();
                }
                catch
                {
                    throw;
                }
                _continuousVelocity = value;
            }
        }



        void _statusTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            int responseCode = 1;

            try
            {
                string response = "";
                lock (_statusLock)
                {
                    _serialPort.DiscardInBuffer();
                    _serialPort.WriteLine("SC");
                    response = _serialPort.ReadLine();
                }
                if (response.Length == 7)
                {
                    response = response.Substring(3);
                    responseCode = (Convert.ToInt32(response.Substring(2, 1)) % 2);
                }
            }
            catch (Exception /*ex*/)
            {
                // if the check fails, we will try again
                _statusTimer.Start();
                return;
            }

            if (responseCode != 0)
            {
                if (!_driveBusy)
                {
                    _driveBusy = true;
                    OnDriveNotReady(null);
                }
                _statusTimer.Start();
            }
            else
            {
                if (_driveBusy)
                {
                    //when polling very rapidly, the timer elapsed will overlap
                    //so don't keep indicating the motor has stopped
                    _driveBusy = false;
                    OnDriveReady(null);
                }
            }
            if(_motortype == MotorType.Linear)
                CheckLimitHoming();
        }

        void OnDriveNotReady(EventArgs e)
        {
            if (DriveNotReady != null)
                DriveNotReady(null, e);
        }

        void OnDriveReady(EventArgs e)
        {
            if (DriveReady != null)
                DriveReady(null, e);
        }

    }
}
