Sub ChangeColor(columnIndex,sheetName)

Sheets(sheetName).Activate
Range(columnIndex).Font.Color = vbRed

Range("A1:K1").Font.Bold = True

End Sub