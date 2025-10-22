using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Web;

public class VnPayLibrary
{
    public const string VERSION = "2.1.0";
    private readonly SortedList<string, string> _requestData = new(new VnPayCompare());
    private readonly SortedList<string, string> _responseData = new(new VnPayCompare());

    public void AddRequestData(string key, string value)
    {
        if (!string.IsNullOrEmpty(key) && !string.IsNullOrEmpty(value))
        {
            _requestData[key] = value;
        }
    }

    public void AddResponseData(string key, string value)
    {
        if (!string.IsNullOrEmpty(key) && !string.IsNullOrEmpty(value))
        {
            _responseData[key] = value;
        }
    }

    public string GetResponseData(string key)
    {
        return _responseData.TryGetValue(key, out string? value) ? value : string.Empty;
    }

    public string CreateRequestUrl(string baseUrl, string vnpHashSecret)
    {
        if (string.IsNullOrEmpty(baseUrl))
            throw new ArgumentException("baseUrl không được null hoặc rỗng", nameof(baseUrl));

        if (string.IsNullOrEmpty(vnpHashSecret))
            throw new ArgumentException("vnpHashSecret không được null hoặc rỗng", nameof(vnpHashSecret));

        StringBuilder data = new();
        
        foreach (KeyValuePair<string, string> kv in _requestData)
        {
            if (!string.IsNullOrEmpty(kv.Value))
            {
                data.Append(HttpUtility.UrlEncode(kv.Key) + "=" + HttpUtility.UrlEncode(kv.Value) + "&");
            }
        }

        if (data.Length == 0)
            throw new InvalidOperationException("Không có dữ liệu yêu cầu để tạo URL");

        string queryString = data.ToString();
        
        // Remove last '&'
        if (queryString.EndsWith("&"))
        {
            queryString = queryString.Remove(queryString.Length - 1);
        }

        string signData = queryString;
        string vnpSecureHash = HmacSHA512(vnpHashSecret, signData);
        
        string paymentUrl = baseUrl + "?" + queryString + "&vnp_SecureHash=" + vnpSecureHash;

        return paymentUrl;
    }

    public bool ValidateSignature(string inputHash, string secretKey)
    {
        if (string.IsNullOrEmpty(inputHash))
            return false;

        if (string.IsNullOrEmpty(secretKey))
            return false;

        StringBuilder rspRaw = new();
        
        foreach (KeyValuePair<string, string> kv in _responseData)
        {
            if (!string.IsNullOrEmpty(kv.Value) && 
                kv.Key != "vnp_SecureHashType" && 
                kv.Key != "vnp_SecureHash")
            {
                rspRaw.Append(HttpUtility.UrlEncode(kv.Key) + "=" + HttpUtility.UrlEncode(kv.Value) + "&");
            }
        }

        if (rspRaw.Length == 0)
            return false;

        string signData = rspRaw.ToString();
        
        // Remove last '&'
        if (signData.EndsWith("&"))
        {
            signData = signData.Remove(signData.Length - 1);
        }

        string checkSum = HmacSHA512(secretKey, signData);
        
        return checkSum.Equals(inputHash, StringComparison.InvariantCultureIgnoreCase);
    }

    private static string HmacSHA512(string key, string inputData)
    {
        if (string.IsNullOrEmpty(key))
            throw new ArgumentException("Khóa không được null hoặc rỗng", nameof(key));

        if (string.IsNullOrEmpty(inputData))
            throw new ArgumentException("Dữ liệu đầu vào không được null hoặc rỗng", nameof(inputData));

        byte[] keyBytes = Encoding.UTF8.GetBytes(key);
        byte[] inputBytes = Encoding.UTF8.GetBytes(inputData);
        
        using var hmac = new HMACSHA512(keyBytes);
        byte[] hashValue = hmac.ComputeHash(inputBytes);
        return BitConverter.ToString(hashValue).Replace("-", "").ToLower();
    }
}

public class VnPayCompare : IComparer<string>
{
    public int Compare(string? x, string? y)
    {
        if (x == y) return 0;
        if (x == null) return -1;
        if (y == null) return 1;
        
        int vnpCompare = string.Compare(x, y, StringComparison.InvariantCultureIgnoreCase);
        return vnpCompare != 0 ? vnpCompare : string.Compare(x, y, StringComparison.Ordinal);
    }
}