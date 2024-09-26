using System.Collections.Generic;
using UnityEngine;

namespace RTLTMPro {
  public static class LigatureFixer {
    private static readonly List<int> _ltrTextHolder = new List<int>(512);
    private static readonly List<int> _tagTextHolder = new List<int>(512);


    private static readonly HashSet<char>
        _mirroredCharsSet = new HashSet<char>(MirroredCharsMaper.MirroredCharsMap.Keys);

    private static void FlushBufferToOutputReverse(List<int> buffer, FastStringBuilder output) {
      for (int j = 0; j < buffer.Count; j++) {
        output.Append(buffer[buffer.Count - 1 - j]);
      }

      buffer.Clear();
    }
    private static void FlushBufferToOutput(List<int> buffer, FastStringBuilder output) {
      for (int j = 0; j < buffer.Count; j++) {
        output.Append(buffer[j]);
      }
      buffer.Clear();
    }

    /// <summary>
    /// Fixes the flow of the text.
    /// </summary>
    public static void Fix(FastStringBuilder input, List<(int, int)> tags, FastStringBuilder output,
        bool farsi, bool fixTextTags, bool preserveNumbers) {
      // Some texts like tags and English words need to be displayed in their original order.
      // This list keeps the characters that their order should be reserved
      // and streams reserved texts into final letters.
      _ltrTextHolder.Clear();
      _tagTextHolder.Clear();
      var (inputCharacterType, inputType) =
          MixedTypographer.CharactersTypeDetermination(input, tags, fixTextTags);
      // Tips:
      // Process is invert order, so the export text is invert in default
      for (int i = input.Length - 1; i >= 0; i--) {
        bool isInMiddle = i > 0 && i < input.Length - 1;
        bool isAtBeginning = i == 0;
        bool isAtEnd = i == input.Length - 1;

        int characterAtThisIndex = input.Get(i);

        int nextCharacter = default;

        if (!isAtEnd)
          nextCharacter = input.Get(i + 1);

        int previousCharacter = default;
        if (!isAtBeginning)
          previousCharacter = input.Get(i - 1);

        #region process with Tags


        if (inputCharacterType[i] == ContextType.Tag) {
          int nextI = i;
          // TagTextHolder is a List that stores tag contents in LTR order
          // The final order of the tag contents needs to be the same RTL order as the Arabic text
          // Therefore, during the entire Text processing,
          // the tag contents must be adjusted to RTL order in the last cell
          _tagTextHolder.Add(characterAtThisIndex);
          for (int j = i - 1; j >= 0; j--) {
            if (inputCharacterType[j] != ContextType.Tag) {
              nextI = j;
              break;
            }
            int jChar = input.Get(j);
            _tagTextHolder.Add(jChar);
            if (j == 0) nextI = -1;
          }
          FlushBufferToOutputReverse(_ltrTextHolder, output);
          FlushBufferToOutput(_tagTextHolder, output);
          
          i = nextI + 1 ;
          continue;
        }
        #endregion

        #region process with Punctutaion and Symbol || Mirrored Chars

        if (Char32Utils.IsPunctuation(characterAtThisIndex) 
            || Char32Utils.IsSymbol(characterAtThisIndex) ||
            characterAtThisIndex == ' ') {
          ContextType characterType = inputCharacterType[i];
          if (_mirroredCharsSet.Contains((char)characterAtThisIndex) 
              && characterType == ContextType.RightToLeft) {
            characterAtThisIndex = MirroredCharsMaper.MirroredCharsMap[(char)characterAtThisIndex];
            FlushBufferToOutputReverse(_ltrTextHolder, output);
            output.Append(characterAtThisIndex);
            continue;
          }
          // fixed: refer to inputCharacterType to process Character

          if (characterType == ContextType.RightToLeft) {
            FlushBufferToOutputReverse(_ltrTextHolder, output);
            output.Append(characterAtThisIndex);
            continue;
          }

          if (characterType == ContextType.LeftToRight) {
            _ltrTextHolder.Add(characterAtThisIndex);
            continue;
          }

          if (characterType == ContextType.Default) {
            Debug.LogError($"Error Character Type Process,index:{i},Text:{input}," +
                           $"Text char array:{input.ToString().ToCharArray()}");
            if (inputType == ContextType.RightToLeft) {
              FlushBufferToOutputReverse(_ltrTextHolder, output);
              output.Append(characterAtThisIndex);
              continue;
            } else {
              _ltrTextHolder.Add(characterAtThisIndex);
              continue;
            }
          }
        }

        #endregion

        if (isInMiddle) {
          bool isAfterEnglishChar = Char32Utils.IsEnglishLetter(previousCharacter);
          bool isBeforeEnglishChar = Char32Utils.IsEnglishLetter(nextCharacter);
          bool isAfterNumber = Char32Utils.IsNumber(previousCharacter, preserveNumbers, farsi);
          bool isBeforeNumber = Char32Utils.IsNumber(nextCharacter, preserveNumbers, farsi);
          bool isAfterSymbol = Char32Utils.IsSymbol(previousCharacter);
          bool isBeforeSymbol = Char32Utils.IsSymbol(nextCharacter);

          // For cases where english words and farsi/arabic are mixed.
          // This allows for using farsi/arabic, english and numbers in one sentence.
          // If the space is between numbers,symbols or English words, keep the order
          if (characterAtThisIndex == ' ' &&
              (isBeforeEnglishChar || isBeforeNumber || isBeforeSymbol) &&
              (isAfterEnglishChar || isAfterNumber || isAfterSymbol)) {
            _ltrTextHolder.Add(characterAtThisIndex);
            continue;
          }
        }

        if (Char32Utils.IsLetter(characterAtThisIndex) && 
            !Char32Utils.IsRTLCharacter(characterAtThisIndex) ||
            Char32Utils.IsNumber(characterAtThisIndex, preserveNumbers, farsi)) {
          _ltrTextHolder.Add(characterAtThisIndex);
          continue;
        }

        // Handle surrogates
        if (characterAtThisIndex >= (char)0xD800 &&
            characterAtThisIndex <= (char)0xDBFF ||
            characterAtThisIndex >= (char)0xDC00 && characterAtThisIndex <= (char)0xDFFF) {
          _ltrTextHolder.Add(characterAtThisIndex);
          continue;
        }

        FlushBufferToOutputReverse(_ltrTextHolder, output);

        if (characterAtThisIndex != 0xFFFF &&
            characterAtThisIndex != (int)SpecialCharacters.ZeroWidthNoJoiner) {
          output.Append(characterAtThisIndex);
        }
      }

      FlushBufferToOutputReverse(_ltrTextHolder, output);
    }
  }
}
