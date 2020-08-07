using System;
using System.Collections.Generic;
using System.Text;

namespace MarineBot.Helpers
{
    internal static class FacesHelper
    {
        const string PageUri = "https://thecocoyard.com/doom/";
        public static string GetIdleFace()
        {
            string[] faces = { "Idle_0.png", "Left_0.png", "Right_0.png" };
            return PageUri + RandomEntry(faces);
        }

        public static string GetErrorFace()
        {
            string[] faces = { "Angry_4.png", "Idle_4.png", "Ouch_0.png", "Ouch_1.png", "Left_4.png",
                               "Ouch_2.png", "Ouch_3.png", "Ouch_4.png", "Right_4.png"};
            return PageUri + RandomEntry(faces);
        }

        public static string GetSuccessFace()
        {
            string[] faces = { "Smile_0.png", "Smile_1.png", "Smile_2.png", "Smile_3.png"};
            return PageUri + RandomEntry(faces);
        }

        public static string GetWarningFace()
        {
            string[] faces = { "Idle_2.png", "Left_2.png", "Right_2.png", "Angry_0.png", "Angry_1.png",
                               "Angry_2.png"};
            return PageUri + RandomEntry(faces);
        }

        public static string GetFace(string face)
        {
            return PageUri + face;
        }

        static string RandomEntry(string[] arr)
        {
            Random random = new Random();
            return arr[random.Next(0, arr.Length)];
        }
    }
}
