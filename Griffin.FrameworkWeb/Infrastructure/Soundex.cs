using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace GriffinFrameworkWeb.Infrastructure
{
    public static class Soundex
    {
        public static string For(string word)
        {
            const int MaxSoundexCodeLength = 4;

            var soundexCode = new StringBuilder();
            var previousWasHOrW = false;

            word = Regex.Replace(
                word == null ? string.Empty : word.ToUpper(),
                @"[^\w\s]",
                string.Empty);

            if (string.IsNullOrEmpty(word))
                return string.Empty.PadRight(MaxSoundexCodeLength, '0');

            soundexCode.Append(word.First());

            for (var i = 1; i < word.Length; i++)
            {
                var numberCharForCurrentLetter =
                    GetCharNumberForLetter(word[i]);

                if (i == 1 &&
                    numberCharForCurrentLetter ==
                    GetCharNumberForLetter(soundexCode[0]))
                    continue;

                if (soundexCode.Length > 2 && previousWasHOrW &&
                    numberCharForCurrentLetter ==
                    soundexCode[soundexCode.Length - 2])
                    continue;

                if (soundexCode.Length > 0 &&
                    numberCharForCurrentLetter ==
                    soundexCode[soundexCode.Length - 1])
                    continue;

                soundexCode.Append(numberCharForCurrentLetter);

                previousWasHOrW = "HW".Contains(word[i]);
            }

            return soundexCode
                .Replace("0", string.Empty)
                .ToString()
                .PadRight(MaxSoundexCodeLength, '0')
                .Substring(0, MaxSoundexCodeLength);
        }

        private static char GetCharNumberForLetter(char letter)
        {
            if ("BFPV".Contains(letter)) return '1';
            if ("CGJKQSXZ".Contains(letter)) return '2';
            if ("DT".Contains(letter)) return '3';
            if ('L' == letter) return '4';
            if ("MN".Contains(letter)) return '5';
            if ('R' == letter) return '6';

            return '0';
        }
    }
}