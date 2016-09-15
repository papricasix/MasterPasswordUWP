using System.IO;
using System.Linq;
using System.Text;
using Windows.UI;

namespace MasterPasswordUWP.Algorithm
{
    public class IdentIcon
    {
        // https://github.com/Lyndir/MasterPassword/blob/master/MasterPassword/Java/masterpassword-algorithm/src/main/java/com/lyndir/masterpassword/MPIdenticon.java

        private static readonly Color[] Colors = { Windows.UI.Colors.Crimson, Windows.UI.Colors.Green, Windows.UI.Colors.DarkGoldenrod, Windows.UI.Colors.CornflowerBlue, Windows.UI.Colors.DarkOrchid, Windows.UI.Colors.DarkTurquoise, Windows.UI.Colors.Gray/*was: Black*/ };
        private static readonly char[] LeftArm = { '╔', '╚', '╰', '═' };
        private static readonly char[] RightArm = { '╗', '╝', '╯', '═' };
        private static readonly char[] Body = { '█', '░', '▒', '▓', '☺', '☻' };
        private static readonly char[] Accessory = {
            '◈', '◎', '◐', '◑', '◒', '◓', '☀', '☁', '☂', '☃', '☄', '★', '☆', '☎', '☏', '⎈', '⌂', '☘', '☢', '☣', '☕',  '⌚', '⌛', '⏰', '⚡', '⛄', '⛅', '☔', '♔', '♕', '♖', '♗', '♘', '♙', '♚', '♛', '♜', '♝', '♞', '♟', '♨', '♩', '♪', '♫', '⚐', '⚑', '⚔', '⚖', '⚙', '⚠', '⌘', '⏎', '✄', '✆', '✈', '✉', '✌' };

        public Color Color { get; private set; }
        public string Text { get; private set; }

        public IdentIcon(string userName, char[] masterPassword)
        {
            var masterPasswordBytes = Encoding.UTF8.GetBytes(masterPassword);
            var identiconSeedBytes = MasterKey.ComputeHmacOf(masterPasswordBytes, Encoding.UTF8.GetBytes(userName));
            MasterKey.Fill(masterPasswordBytes, (byte)0);

            var identiconSeed = identiconSeedBytes.Select(b => (int)b & 0xff).ToArray();

            Color = Colors[identiconSeed[4] % Colors.Length];
            Text = $"{LeftArm[identiconSeed[0] % LeftArm.Length]}{Body[identiconSeed[1] % Body.Length]}{RightArm[identiconSeed[2] % RightArm.Length]}{Accessory[identiconSeed[3] % Accessory.Length]}";
        }
    }
}
