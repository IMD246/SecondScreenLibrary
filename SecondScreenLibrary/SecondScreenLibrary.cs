using System.Runtime.InteropServices;

namespace SecondScreenLibrary
{
    [StructLayout(LayoutKind.Sequential)]
    public struct ScreenInfoNative
    {
        public IntPtr DeviceName;
        public int Width;
        public int Height;
        public int PositionX;
        public int PositionY;
        [MarshalAs(UnmanagedType.I1)]
        public bool IsPrimary;
    }

    public static class ScreenInterop
    {
        [UnmanagedCallersOnly(EntryPoint = "GetScreens")]
        public static IntPtr GetScreens(IntPtr screenCountPtr)
        {
            List<ScreenInfoNative> screens = [];

            try
            {
                foreach (var screen in Screen.AllScreens)
                {
                    ScreenInfoNative nativeScreen = new()
                    {
                        DeviceName = Marshal.StringToHGlobalAnsi(screen.DeviceName),
                        Width = screen.Bounds.Width,
                        Height = screen.Bounds.Height,
                        PositionX = screen.Bounds.X,
                        PositionY = screen.Bounds.Y,
                        IsPrimary = screen.Primary
                    };

                    screens.Add(nativeScreen);
                }

                int screenCount = screens.Count;
                Marshal.WriteInt32(screenCountPtr, screenCount);
                if (screenCount == 0) return IntPtr.Zero;

                IntPtr listPtr = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(ScreenInfoNative)) * screenCount);
                for (int i = 0; i < screenCount; i++)
                {
                    IntPtr currentPtr = IntPtr.Add(listPtr, i * Marshal.SizeOf(typeof(ScreenInfoNative)));
                    Marshal.StructureToPtr(screens[i], currentPtr, false);
                }

                return listPtr;
            }
            catch
            {
                Marshal.WriteInt32(screenCountPtr, 0);
                return IntPtr.Zero;
            }
        }

        [UnmanagedCallersOnly(EntryPoint = "FreeMemory")]
        public static void FreeMemory(IntPtr ptr)
        {
            if (ptr != IntPtr.Zero)
            {
                Marshal.FreeHGlobal(ptr);
            }
        }
    }
}
