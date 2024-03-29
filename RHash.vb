Module Module1

    Sub Main()

        Dim sArg() As String = Environment.GetCommandLineArgs

        If UBound(sArg) = 0 Then
            Console.ForegroundColor = ConsoleColor.Cyan
            Console.WriteLine("RHash, (re)Written in 2021 by Glenn Larsson. Calculates Richhash from PE header." & vbCrLf & "Parameters:")
            Console.ForegroundColor = ConsoleColor.Green
            Console.WriteLine("1. Filename")
            Console.ForegroundColor = ConsoleColor.Yellow
            Console.WriteLine("Example: RHash C:\Windows\System32\cmd.exe")
            Console.ForegroundColor = ConsoleColor.Green
            Console.WriteLine("The above example would calculate and print the richhash value for C:\Windows\System32\cmd.exe")
            Console.ForegroundColor = ConsoleColor.White
            Exit Sub
        End If

        Dim sFN As String = ""

        Try
            sFN = sArg(1)
        Catch ex As Exception
        End Try

        If System.IO.File.Exists(sFN) = False Then
            Console.WriteLine("File " & sFN & " does not exist.")
            End
        End If

        Dim fl As New System.IO.FileInfo(sFN)
        If fl.Length = 0 Then
            Console.WriteLine("File " & sFN & " is 0 length - or locked. Cant continue.")
            End
        End If
        fl = Nothing



        Dim sTotal As String = ""
        Dim bRichXor(3) As Byte

        Dim h As System.Security.Cryptography.MD5Cng = New System.Security.Cryptography.MD5Cng
        Dim bFileContent(1023) As Byte
        Dim bRichHash(127) As Byte
        Dim bHash() As Byte
        Dim sTags As String = ""

        Dim fs As System.IO.FileStream
        Dim sRichIndicator As String = ""
        Dim iRichHeaderOffset As Integer = 0

        Try
            If FileLen(sFN) > 1023 Then

                fs = System.IO.File.OpenRead(sFN) ' Read 1K byte() array
                fs.Read(bFileContent, 0, 1024)
                fs.Close()

                sRichIndicator = "" ' String of byte valuse, for Instr() search
                For hB = 0 To UBound(bFileContent)
                    sRichIndicator = sRichIndicator & Chr(bFileContent(hB))
                Next

                If bFileContent(0) = Asc("M") And bFileContent(1) = Asc("Z") Then ' PE header

                    iRichHeaderOffset = InStr(1, sRichIndicator, "Rich")
                    iRichHeaderOffset = iRichHeaderOffset - 128 - 1

                    For x = 0 To 3 ' Zero array. Always.
                        bRichXor(x) = 0
                    Next

                    If iRichHeaderOffset < 1 Then ' No header, default to 127 (Full length)
                        iRichHeaderOffset = 127
                    Else ' Header? Extract XOR bytes
                        For x = 0 To 3
                            bRichXor(x) = bFileContent(128 + iRichHeaderOffset + 4 + x) ' 4 bytes, post "Rich" sign
                        Next
                    End If

                    ReDim Preserve bRichHash(iRichHeaderOffset - 1) ' Resize array for proper richheader size, multiple of 4 

                    If UBound(bFileContent) >= 255 Then
                        For b = 0 To (iRichHeaderOffset - 1)
                            bRichHash(b) = bFileContent(128 + b)
                            bRichHash(b) = bRichHash(b) Xor bRichXor(b Mod 4)
                        Next

                        bHash = h.ComputeHash(bRichHash)
                        sTotal = ""
                        For m = 0 To UBound(bHash)
                            sTotal = sTotal & Microsoft.VisualBasic.Right("0" & Hex(bHash(m)), 2)
                        Next

                    End If

                End If

            End If

        Catch ex As Exception
        End Try
        If sTotal = "" Then sTotal = "-" ' Default to "-" if none seen
        Console.WriteLine(LCase(sTotal))

        h = Nothing

    End Sub

End Module
