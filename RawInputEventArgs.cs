using Linearstar.Windows.RawInput;
using System;

class RawInputEventArgs : EventArgs
{
    public RawInputEventArgs(RawInputData data) => Data = data;

    public RawInputData Data { get; }
}