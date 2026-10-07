using System.Text.RegularExpressions;

namespace ManimPilot.Math;

public class BitPlaceValue
{
    public int BitIndex { get; set; } // 0 from right (least significant)
    public char BitChar { get; set; } // '0' or '1'
    public long PlaceValue { get; set; } // 2^BitIndex
    public bool IsActive => BitChar == '1';
    public long Contribution => IsActive ? PlaceValue : 0;
}

public class BinaryToDecimalResult
{
    public string OriginalBinary { get; set; } = string.Empty;
    public string NormalizedBinary { get; set; } = string.Empty;
    public List<BitPlaceValue> BitsFromLeftToRight { get; set; } = new();
    public List<long> ActivePlaceValues { get; set; } = new();
    public string AdditionExpression { get; set; } = string.Empty;
    public long DecimalResult { get; set; }
    public bool IsValid { get; set; }
    public string? ErrorMessage { get; set; }
}

public interface IBinaryMathService
{
    BinaryToDecimalResult ConvertBinaryToDecimal(string binaryInput);
}

public class BinaryMathService : IBinaryMathService
{
    public BinaryToDecimalResult ConvertBinaryToDecimal(string binaryInput)
    {
        if (string.IsNullOrWhiteSpace(binaryInput))
        {
            return new BinaryToDecimalResult
            {
                OriginalBinary = binaryInput ?? string.Empty,
                IsValid = false,
                ErrorMessage = "Binary string cannot be empty."
            };
        }

        // Clean spaces or underscores (e.g. 1011_0110 -> 10110110)
        var cleaned = binaryInput.Replace(" ", "").Replace("_", "").Trim();

        if (!Regex.IsMatch(cleaned, "^[01]+$"))
        {
            return new BinaryToDecimalResult
            {
                OriginalBinary = binaryInput,
                NormalizedBinary = cleaned,
                IsValid = false,
                ErrorMessage = $"Invalid binary string '{binaryInput}'. Only digits 0 and 1 are allowed."
            };
        }

        if (cleaned.Length > 63)
        {
            return new BinaryToDecimalResult
            {
                OriginalBinary = binaryInput,
                NormalizedBinary = cleaned,
                IsValid = false,
                ErrorMessage = "Binary string exceeds maximum 63-bit integer limit."
            };
        }

        var bitList = new List<BitPlaceValue>();
        int length = cleaned.Length;
        long total = 0;
        var activeValues = new List<long>();

        for (int i = 0; i < length; i++)
        {
            int power = length - 1 - i;
            long placeVal = 1L << power;
            char bit = cleaned[i];

            var bitObj = new BitPlaceValue
            {
                BitIndex = power,
                BitChar = bit,
                PlaceValue = placeVal
            };
            bitList.Add(bitObj);

            if (bit == '1')
            {
                total += placeVal;
                activeValues.Add(placeVal);
            }
        }

        string additionExpr = activeValues.Count > 0 ? string.Join(" + ", activeValues) : "0";

        return new BinaryToDecimalResult
        {
            OriginalBinary = binaryInput,
            NormalizedBinary = cleaned,
            BitsFromLeftToRight = bitList,
            ActivePlaceValues = activeValues,
            AdditionExpression = additionExpr,
            DecimalResult = total,
            IsValid = true
        };
    }
}
