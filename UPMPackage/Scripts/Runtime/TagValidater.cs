using System;
using System.Collections.Generic;
using System.Reflection;
using TMPro;

namespace RTLTMPro {
  public partial class RTLTextMeshPro {
    private List<(int,int)> FindTags(string input) {
      var tags = new List<(int,int)>();
      for (int i = 0; i < input.Length; i++) {
        char ch = input[i];
        if (ch == 60)  // '<'
        {
          // Check if Tag is valid. If valid, skip to the end of the validated tag.
          if (ValidateTag(input, i + 1, out int endTagIndex))
          {
            tags.Add((i,endTagIndex));
          }
        }
      }
      return tags;
    }
    private bool ValidateTag(string input, int startIndex, out int endIndex) {
      // 创建 UnicodeChar 的数组
      var unicodeCharType = typeof(TMP_Text).GetNestedType("UnicodeChar",
        BindingFlags.NonPublic | BindingFlags.Instance);
      var unicodeChars = Array.CreateInstance(unicodeCharType, input.Length);
      // 填充数组
      for (int i = 0; i < input.Length; i++) {
        var unicodeChar = Activator.CreateInstance(unicodeCharType);
        unicodeCharType.GetField("unicode").SetValue(unicodeChar, input[i]);
        unicodeCharType.GetField("stringIndex").SetValue(unicodeChar, i);
        unicodeCharType.GetField("length").SetValue(unicodeChar, 1);
        unicodeChars.SetValue(unicodeChar, i);
      }

      MethodInfo method = typeof(TextMeshProUGUI).GetMethod("ValidateHtmlTag",
        BindingFlags.NonPublic | BindingFlags.Instance);
      if (method != null) {
        object[] parameters = { unicodeChars, startIndex, null };
        bool result = (bool)method.Invoke(this, parameters);
        endIndex = (int)parameters[2];
        return result;
      } else {
        {
          endIndex = startIndex;
          return false;
        }
      }
    }
  }
}
