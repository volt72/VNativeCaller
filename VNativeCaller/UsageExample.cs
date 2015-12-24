using JRPC_Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XDevkit;
using XeClutch;

class UsageExample
{
    IXboxConsole Console;
    VNativeCaller Caller;

    void Main()
    {
        if (((IXboxConsole)Console).Connect(out Console))
        {
            // Connected to default Xbox 360 in neighborhood so now let's initialize VNativeCaller..
            Caller = new VNativeCaller(Console);

            // Need to dump the native table after initializing VNativeCaller..
            int found = Caller.DumpNatives((uint)VNativeTableAddresses.TU27); // found = the amount of natives found in the native table that were dumped and organized into our dict

            // Test a native (we're going to edit gravity)
            SET_GRAVITY_LEVEL(0.1f);
        }
    }

    void SET_GRAVITY_LEVEL(float gravityLevel)
    {
        Caller.CallVoid(Caller.JOAAT("SET_GRAVITY_LEVEL"), gravityLevel); // you can use 0x2D833F4A instead of Caller.JOAAT("SET_GRAVITY_LEVEL") because they both return the same
    }
}