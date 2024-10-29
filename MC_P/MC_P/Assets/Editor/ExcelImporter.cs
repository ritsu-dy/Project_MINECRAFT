using System;
using System.Collections.Generic;
using System.IO;
using ExcelDataReader;
using UnityEngine;

public static class ExcelImporter
{
    public static List<Dictionary<string, object>> Import(string filePath)
    {
        var data = new List<Dictionary<string, object>>();
        System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);

        using (var stream = File.Open(filePath, FileMode.Open, FileAccess.Read))
        {
            using (var reader = ExcelReaderFactory.CreateReader(stream))
            {
                // 첫 번째 행을 스킵 (헤더 무시)
                while (reader.Read())
                {
                    if (reader.GetValue(0)?.ToString() == "#") continue;

                    var headers = new List<string>();

                    // 첫 번째 유효한 행에서 헤더 추출
                    for (int i = 0; i < reader.FieldCount; i++)
                    {
                        var headerValue = reader.GetValue(i);
                        if (headerValue != null && headerValue.ToString() != "#")
                        {
                            headers.Add(headerValue.ToString());
                        }
                    }

                    // 다음 행부터 데이터를 읽기 시작
                    while (reader.Read())
                    {
                        if (reader.GetValue(0)?.ToString() == "#") continue;

                        var rowData = new Dictionary<string, object>();

                        for (int i = 0; i < headers.Count; i++)
                        {
                            if (i >= reader.FieldCount) continue;

                            var header = headers[i];
                            var value = reader.GetValue(i);

                            if (value != null)
                            {
                                rowData[header] = value;
                            }
                            else
                            {
                                rowData[header] = "";
                            }
                        }

                        data.Add(rowData);
                    }
                    break; // 헤더와 데이터 읽기 완료 후 루프 종료
                }
            }
        }

        return data;
    }
}
