// The SeedV Lab (Beijing SeedV Technology Co., Ltd.) modifications: Copyright 2024 The SeedV Lab
// (Beijing SeedV Technology Co., Ltd.) All Rights Reserved. The modifications in this file are the intellectual property of the SeedV Lab.

// The modifications in this file are the intellectual property of the SeedV Lab and are governed by
// the same license terms as the original sourcecode

using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace RTLTMPro {
  public static class LigatureFixer {
    private static readonly List<int> _ltrTextHolder = new List<int>(512);
    private static readonly List<int> _tagTextHolder = new List<int>(512);


    private static readonly HashSet<char>
        _mirroredCharsSet = new HashSet<char>(MirroredCharsMaper.MirroredCharsMap.Keys);

    private static void FlushBufferToOutput(List<int> buffer, FastStringBuilder output) {
      for (int j = 0; j < buffer.Count; j++) {
        output.Append(buffer[buffer.Count - 1 - j]);
      }

      buffer.Clear();
    }

    /// <summary>
    /// Fixes the flow of the text.
    /// </summary>
    public static void Fix(FastStringBuilder input, FastStringBuilder output, 
        bool farsi, bool fixTextTags, bool preserveNumbers) {
      // Some texts like tags, English words and numbers need to be displayed in their original order.
      // This list keeps the characters that their order should be reserved and streams reserved texts into final letters.
      _ltrTextHolder.Clear();
      _tagTextHolder.Clear();
      var (inputCharacterType, inputType) = MixedTypographer.CharactersTypeDetermination(input);
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

        if (fixTextTags) {
          if (characterAtThisIndex == '>') {
            // We need to check if it is actually the beginning of a tag.
            bool isValidTag = false;
            int nextI = i;

            // TagTextHolder is a List for hold on the tag content sequence
            // Tag content's final sequence is same with arabic text
            // in this whole Process,Tag content will be revert sequence in last unit
            // so in this unit,Tag content's sequence is same with English
            _tagTextHolder.Add(characterAtThisIndex);

            for (int j = i - 1; j >= 0; j--) {
              var jChar = input.Get(j);

              _tagTextHolder.Add(jChar);

              if (jChar == '<') {
                // TODO: Tag Valid judgment is to simple
                // other invalid condition also need consider 
                var jPlus1Char = input.Get(j + 1);
                // Tags do not start with space
                if (jPlus1Char == ' ') {
                  break;
                }

                isValidTag = true;
                nextI = j;
                break;
              }
            }
            
            if (isValidTag) {
              // if a Tag is find and the Tag content is valid.
              // fixer going to a new split.
              // first thing is pushing the LTR list buffer into output buffer
              // then get Tag content's reverse text and push into output buffer
              // if the Tag content is invalid
              // fixer continue work in old split
              // Bug: if tag is between two English text
              // this tag push process shuffle English text 
              FlushBufferToOutput(_ltrTextHolder, output);
              FlushBufferToOutput(_tagTextHolder, output);
              i = nextI;
              continue;
            } else {
              _tagTextHolder.Clear();
            }
          }
        }

        #endregion

        #region process with Punctutaion and Symbol || Mirrored Chars

        if (Char32Utils.IsPunctuation(characterAtThisIndex) || Char32Utils.IsSymbol(characterAtThisIndex) ||
            characterAtThisIndex == ' ') {
          ContextType characterType = inputCharacterType[i];
          if (_mirroredCharsSet.Contains((char)characterAtThisIndex) && characterType == ContextType.RightToLeft) {
            characterAtThisIndex = MirroredCharsMaper.MirroredCharsMap[(char)characterAtThisIndex];
            FlushBufferToOutput(_ltrTextHolder, output);
            output.Append(characterAtThisIndex);
            continue;
          }
          // fixed: refer to inputCharacterTpye to process Character

          if (characterType == ContextType.RightToLeft) {
            FlushBufferToOutput(_ltrTextHolder, output);
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
              FlushBufferToOutput(_ltrTextHolder, output);
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

          // For cases where english words and farsi/arabic are mixed. This allows for using farsi/arabic, english and numbers in one sentence.
          // If the space is between numbers,symbols or English words, keep the order
          if (characterAtThisIndex == ' ' &&
              (isBeforeEnglishChar || isBeforeNumber || isBeforeSymbol) &&
              (isAfterEnglishChar || isAfterNumber || isAfterSymbol)) {
            _ltrTextHolder.Add(characterAtThisIndex);
            continue;
          }
        }

        if (Char32Utils.IsLetter(characterAtThisIndex) && !Char32Utils.IsRTLCharacter(characterAtThisIndex) ||
            Char32Utils.IsNumber(characterAtThisIndex, preserveNumbers, farsi)) {
          _ltrTextHolder.Add(characterAtThisIndex);
          continue;
        }

        // handle surrogates
        if (characterAtThisIndex >= (char)0xD800 &&
            characterAtThisIndex <= (char)0xDBFF ||
            characterAtThisIndex >= (char)0xDC00 && characterAtThisIndex <= (char)0xDFFF) {
          _ltrTextHolder.Add(characterAtThisIndex);
          continue;
        }

        FlushBufferToOutput(_ltrTextHolder, output);

        if (characterAtThisIndex != 0xFFFF &&
            characterAtThisIndex != (int)SpecialCharacters.ZeroWidthNoJoiner) {
          output.Append(characterAtThisIndex);
        }
      }

      FlushBufferToOutput(_ltrTextHolder, output);
    }
  }
}
