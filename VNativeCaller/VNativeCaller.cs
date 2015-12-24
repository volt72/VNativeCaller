/* VNativeCaller
 * 
 * .NET library used to call native functions used by scripts in GTA V.
 * NOTE: Some natives will not work as they need to be called from certain locations.
 * 
 * Credit to Xx jAmes t xX for JRPC.
 * Credit to proditaki for CallStruct offsets.
 * Credit to XeClutch for everything else.
 */

using JRPC_Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XDevkit;

namespace XeClutch
{
    public enum VNativeTableAddresses : uint
    {
        TU23 = 0x83DCC0D8,
        TU24 = 0x83DCC9D8,
        TU25 = 0x83DDC8F8,
        TU26 = 0x83DDC8F8,
        TU27 = 0x83DDCD08,
    }
    public struct Vector3
    {
        public float x, y, z;
    }

    /// <summary>
    /// .NET library used to call native functions used by scripts in GTA V.
    /// NOTE: Some natives will not work as they need to be called from certain locations.
    /// </summary>
    public class VNativeCaller
    {
        private IXboxConsole Console;
        private Dictionary<uint, uint> Natives = new Dictionary<uint, uint>();

        private const uint CallStruct_ReturnPtr = 0x83B66460;
        private const uint CallStruct_ArgumentCount = 0x83B66464;
        private const uint CallStruct_ArgumentPtr = 0x83B66468;
        private const uint CallStruct_ReturnArray = 0x83B66470;
        private const uint CallStruct_ArgumentArray = 0x83B66480;
        private const uint VNativeCaller_StringAddress = 0x83BAE400;

        /// <summary>
        /// Initializes the VNativeCaller library.
        /// </summary>
        /// <param name="JRPC">JRPC instance that's connected to a console.</param>
        public VNativeCaller(IXboxConsole JRPC)
        {
            Console = JRPC;
        }

        /// <summary>
        /// Call a GTA V native remotely.
        /// </summary>
        /// <param name="NativeNameHash">32-bit Jenkins hash of the native name.</param>
        /// <param name="Arguments">Arguments to be passed through the native call.</param>
        /// <returns></returns>
        private uint[] CallNative(uint NativeNameHash, params object[] Arguments)
        {
            uint[] ret = new uint[3];
            uint string_count = 0;

            uint addr = 0;
            if (!Natives.TryGetValue(NativeNameHash, out addr))
                throw new Exception("Native name hash doesn't exist in native table dump.");

            byte[] nullstr = new byte[0x90];
            Console.WriteByte(VNativeCaller_StringAddress, nullstr);
            for (uint i = 0; i < Arguments.Length; i++)
            {
                object Argument = Arguments[i];
                if (Argument is string)
                {
                    uint str = VNativeCaller_StringAddress + (string_count * 0x30);
                    Console.WriteString(str, (string)Argument + "\0");
                    string_count++;

                    Console.WriteUInt32(CallStruct_ArgumentArray + (i * 4), str);
                }
                else if (Argument is float)
                {
                    Console.WriteFloat(CallStruct_ArgumentArray + (i * 4), (float)Argument);
                }
                else
                {
                    Console.WriteUInt32(CallStruct_ArgumentArray + (i * 4), (uint)Argument);
                }
            }
            Console.WriteUInt32(CallStruct_ArgumentCount, (uint)Arguments.Length);
            Console.WriteUInt32(CallStruct_ArgumentPtr, CallStruct_ArgumentArray);
            Console.WriteUInt32(CallStruct_ReturnPtr, CallStruct_ReturnArray);
            Console.CallVoid(addr, CallStruct_ReturnPtr);

            ret[0] = Console.ReadUInt32(CallStruct_ReturnArray);
            ret[1] = Console.ReadUInt32(CallStruct_ReturnArray + 4);
            ret[2] = Console.ReadUInt32(CallStruct_ReturnArray + 8);
            return ret;
        }

        /// <summary>
        /// Call a GTA V native remotely.
        /// </summary>
        /// <param name="NativeNameHash">32-bit Jenkins hash of the native name.</param>
        /// <param name="Arguments">Arguments to be passed through the native call.</param>
        /// <returns></returns>
        public bool CallBool(uint NativeNameHash, params object[] Arguments)
        {
            return CallNative(NativeNameHash, Arguments)[0] == 1;
        }
        /// <summary>
        /// Call a GTA V native remotely.
        /// </summary>
        /// <param name="NativeNameHash">32-bit Jenkins hash of the native name.</param>
        /// <param name="Arguments">Arguments to be passed through the native call.</param>
        /// <returns></returns>
        public float CallFloat(uint NativeNameHash, params object[] Arguments)
        {
            return (float)CallNative(NativeNameHash, Arguments)[0];
        }
        /// <summary>
        /// Call a GTA V native remotely.
        /// </summary>
        /// <param name="NativeNameHash">32-bit Jenkins hash of the native name.</param>
        /// <param name="Arguments">Arguments to be passed through the native call.</param>
        /// <returns></returns>
        public int CallInt(uint NativeNameHash, params object[] Arguments)
        {
            return (int)CallNative(NativeNameHash, Arguments)[0];
        }
        /// <summary>
        /// Call a GTA V native remotely.
        /// </summary>
        /// <param name="NativeNameHash">32-bit Jenkins hash of the native name.</param>
        /// <param name="Arguments">Arguments to be passed through the native call.</param>
        /// <returns></returns>
        public Vector3 CallVector3(uint NativeNameHash, params object[] Arguments)
        {
            uint[] ret = CallNative(NativeNameHash, Arguments);
            Vector3 vec;
            vec.x = (float)ret[0];
            vec.y = (float)ret[1];
            vec.z = (float)ret[2];
            return vec;
        }
        /// <summary>
        /// Call a GTA V native remotely.
        /// </summary>
        /// <param name="NativeNameHash">32-bit Jenkins hash of the native name.</param>
        /// <param name="Arguments">Arguments to be passed through the native call.</param>
        /// <returns></returns>
        public void CallVoid(uint NativeNameHash, params object[] Arguments)
        {
            CallNative(NativeNameHash, Arguments);
        }

        /// <summary>
        /// Dumps and organizes all GTA V native name hashes with their call addresses.
        /// </summary>
        /// <param name="NativeTable">Location of the native table in memory.</param>
        /// <param name="TableSize">Amount of native name hashes are paired with call addresses in the native table.</param>
        /// <returns>The amount of natives dumped and organized.</returns>
        public int DumpNatives(uint NativeTable, uint TableSize = 6000)
        {
            Natives.Clear();
            int NativesFound = 0;
            for (uint i = 0; i < TableSize; i++)
            {
                uint hash = Console.ReadUInt32(NativeTable + (i * 8));
                uint addr = Console.ReadUInt32(NativeTable + (i * 8) + 4);

                if (hash == 0)
                    break;

                Natives.Add(hash, addr);
                NativesFound++;
            }
            return NativesFound;
        }

        /// <summary>
        /// Generates a Jenkins hash based on the key provided.
        /// </summary>
        /// <param name="Key">The text you wanted hashed.</param>
        /// <returns></returns>
        public uint JOAAT(string Key)
        {
            uint num = 0;
            byte[] bytes = Encoding.UTF8.GetBytes(Key.ToLower());
            for (int i = 0; i < bytes.Length; i++)
            {
                num += bytes[i];
                num += num << 10;
                num ^= num >> 6;
            }
            num += num << 3;
            num ^= num >> 11;
            return (num + (num << 15));
        }
    }
}