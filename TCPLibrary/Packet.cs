﻿using System.Text;

namespace TCPLibrary
{
    /// <summary>Sent from server to client.</summary>
    public enum ResponsePacket
    {
        Handshake,
        Generate,
        UseCode,
        Success,
        Error
    }

    /// <summary>Sent from client to server.</summary>
    public enum RequestPacket
    {
        Generate,
        UseCode
    }

    public class Packet : IDisposable
    {
        List<byte> buffer;
        byte[] readableBuffer;
        int readPos;

        public Packet()
        {
            buffer = new List<byte>(); 
            readPos = 0; 
        }

        public Packet(int _id)
        {
            buffer = new List<byte>(); 
            readPos = 0; 

            Write(_id); 
        }

        public Packet(byte[] _data)
        {
            buffer = new List<byte>(); 
            readPos = 0; 

            SetBytes(_data);
        }

        public void SetBytes(byte[] _data)
        {
            Write(_data);
            readableBuffer = buffer.ToArray();
        }

        public void WriteLength()
        {
            buffer.InsertRange(0, BitConverter.GetBytes(buffer.Count)); 
        }

        public void InsertInt(int _value)
        {
            buffer.InsertRange(0, BitConverter.GetBytes(_value));
        }

        public byte[] ToArray()
        {
            readableBuffer = buffer.ToArray();
            return readableBuffer;
        }

        public int Length()
        {
            return buffer.Count;
        }

        public int UnreadLength()
        {
            return Length() - readPos;
        }

        public void Reset(bool _shouldReset = true)
        {
            if (_shouldReset)
            {
                buffer.Clear();
                readableBuffer = null;
                readPos = 0;
            }
            else
            {
                readPos -= 4;
            }
        }

        public void Write(byte _value)
        {
            buffer.Add(_value);
        }
        public void Write(byte[] _value)
        {
            buffer.AddRange(_value);
        }
        public void Write(short _value)
        {
            buffer.AddRange(BitConverter.GetBytes(_value));
        }
        public void Write(int _value)
        {
            buffer.AddRange(BitConverter.GetBytes(_value));
        }
        public void Write(long _value)
        {
            buffer.AddRange(BitConverter.GetBytes(_value));
        }
        public void Write(float _value)
        {
            buffer.AddRange(BitConverter.GetBytes(_value));
        }
        public void Write(bool _value)
        {
            buffer.AddRange(BitConverter.GetBytes(_value));
        }
        public void Write(string _value)
        {
            Write(_value.Length); 
            buffer.AddRange(Encoding.ASCII.GetBytes(_value)); 
        }

        public byte ReadByte(bool _moveReadPos = true)
        {
            if (buffer.Count > readPos)
            {
               
                byte _value = readableBuffer[readPos]; 
                if (_moveReadPos)
                {
                    readPos += 1; 
                }
                return _value;
            }
            else
            {
                throw new Exception("Could not read value of type 'byte'!");
            }
        }

        public byte[] ReadBytes(int _length, bool _moveReadPos = true)
        {
            if (buffer.Count > readPos)
            {

                byte[] _value = buffer.GetRange(readPos, _length).ToArray(); 
                if (_moveReadPos)
                {
                    readPos += _length; 
                }
                return _value; 
            }
            else
            {
                throw new Exception("Could not read value of type 'byte[]'!");
            }
        }

        public short ReadShort(bool _moveReadPos = true)
        {
            if (buffer.Count > readPos)
            {
                short _value = BitConverter.ToInt16(readableBuffer, readPos); 
                if (_moveReadPos)
                {
                    readPos += 2; 
                }
                return _value; 
            }
            else
            {
                throw new Exception("Could not read value of type 'short'!");
            }
        }

        public int ReadInt(bool _moveReadPos = true)
        {
            if (buffer.Count > readPos)
            {

                int _value = BitConverter.ToInt32(readableBuffer, readPos); 
                if (_moveReadPos)
                {
                    readPos += 4; 
                }
                return _value;
            }
            else
            {
                throw new Exception("Could not read value of type 'int'!");
            }
        }

        public long ReadLong(bool _moveReadPos = true)
        {
            if (buffer.Count > readPos)
            {
                long _value = BitConverter.ToInt64(readableBuffer, readPos); 
                if (_moveReadPos)
                {
                    readPos += 8;
                }
                return _value;
            }
            else
            {
                throw new Exception("Could not read value of type 'long'!");
            }
        }

        public float ReadFloat(bool _moveReadPos = true)
        {
            if (buffer.Count > readPos)
            {
                float _value = BitConverter.ToSingle(readableBuffer, readPos); 
                if (_moveReadPos)
                {
                    readPos += 4; 
                }
                return _value;
            }
            else
            {
                throw new Exception("Could not read value of type 'float'!");
            }
        }
        public bool ReadBool(bool _moveReadPos = true)
        {
            if (buffer.Count > readPos)
            {
                bool _value = BitConverter.ToBoolean(readableBuffer, readPos);
                if (_moveReadPos)
                {
                    readPos += 1; 
                }
                return _value;
            }
            else
            {
                throw new Exception("Could not read value of type 'bool'!");
            }
        }

        public string ReadString(bool _moveReadPos = true)
        {
            try
            {
                int _length = ReadInt(); 
                string _value = Encoding.ASCII.GetString(readableBuffer, readPos, _length); 
                if (_moveReadPos && _value.Length > 0)
                {
                    readPos += _length;
                }
                return _value;
            }
            catch
            {
                throw new Exception("Could not read value of type 'string'!");
            }
        }

        bool disposed = false;

        protected virtual void Dispose(bool _disposing)
        {
            if (!disposed)
            {
                if (_disposing)
                {
                    buffer = null;
                    readableBuffer = null;
                    readPos = 0;
                }

                disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
