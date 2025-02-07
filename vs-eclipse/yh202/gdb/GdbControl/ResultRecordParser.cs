using System;
using System.Collections.Generic;

namespace GdbControl
{
    public class ResultRecordParser
    {
        private int NextParsePos { get; set; }
        private string StringToParse { get; set; }
        private IDictionary<string, object> ResultDict { get; set; }
        private string CurrentEntryKey { get; set; }
        private object CurrentEntryValue { get; set; }
        private bool ParsingNotFinishedYet { get; set; }

        public IDictionary<string, object> Parse(string resultRecordString, int startPos = 0)
        {
            if (ParsingNotFinishedYet)
                throw new InvalidOperationException("reentrant call to Parse");

            ParsingNotFinishedYet = true;
            
            StringToParse = resultRecordString;
            ResultDict = new Dictionary<string, object>();
            NextParsePos = startPos;
            CurrentEntryKey = null;

            while (ParsingNotFinishedYet)
                ParseNextEntry();

            return ResultDict;
        }

        private void ParseNextEntry()
        {
            SkipOpeningBraceOrSeparatorIfAny();
            if (DetectPossibleEndOfRecordAndReact())
                return;

            ExtractCurrentEntryKey();
            ExtractCurrentEntryValue();
            ResultDict.Add(CurrentEntryKey, CurrentEntryValue);
        }

        private void ExtractCurrentEntryValue()
        {
            switch (StringToParse[NextParsePos])
            {
            case '"':
                ParseSingleValue();
                break;
            case '[':
                ParseArray();
                break;
            case '{':
                ParseRecord();
                break;
            }
        }

        private void ExtractCurrentEntryKey()
        {
            var partEndPos = StringToParse.IndexOf('=', NextParsePos); // search the end-position of the key
            CurrentEntryKey = StringToParse.Substring(NextParsePos, partEndPos - NextParsePos);

            NextParsePos = partEndPos + 1;
        }

        private bool DetectPossibleEndOfRecordAndReact()
        {
            if ((NextParsePos < StringToParse.Length) && (StringToParse[NextParsePos] == '}')) // end of record
            {
                NextParsePos++; // we have to move on the parse-position, because this could have been a nested record
                ParsingNotFinishedYet = false;
                return true;
            }

            if (NextParsePos >= StringToParse.Length) // end of string
            {
                ParsingNotFinishedYet = false;
                return true;
            }
            return false;
        }

        private void SkipOpeningBraceOrSeparatorIfAny()
        {
            while (
                (NextParsePos < StringToParse.Length) &&
                ((StringToParse[NextParsePos] == ',') || (StringToParse[NextParsePos] == '{')))
                NextParsePos++;
        }

        private void ParseSingleValue()
        {
            NextParsePos++; // skip the leading quote
            var partEndPos = StringToParse.IndexOf('"', NextParsePos);
            CurrentEntryValue = StringToParse.Substring(NextParsePos, partEndPos - NextParsePos);
            NextParsePos = partEndPos + 1;
        }

        private void ParseArray()
        {
            var resultArray = new List<object>();
            
            NextParsePos++; // skip the opening brace of the array

            while ((NextParsePos < StringToParse.Length) && (StringToParse[NextParsePos] != ']'))
            {
                ExtractCurrentEntryValue();
                resultArray.Add(CurrentEntryValue);
                if ((NextParsePos < StringToParse.Length) && (StringToParse[NextParsePos] == ','))
                    NextParsePos++;
            }

            CurrentEntryValue = resultArray;
            NextParsePos++; // move on past the closing brace of the array
        }

        private void ParseRecord()
        {
            var nestedParser = new ResultRecordParser();
            CurrentEntryValue = nestedParser.Parse(StringToParse, NextParsePos);
            NextParsePos = nestedParser.NextParsePos; // don't forget to sync on the next parse position!
        }
    }
}
