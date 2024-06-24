using System;
using System.IO;
using System.Reflection;
using System.Xml;

namespace PacketGenerator
{
    class Program
    {
        // 각 패킷매니저에 등록할 메시지 정보 변수
        static string clientRegister;   
        static string serverRegister;

        static void Main(string[] args)
        {
            string file = "../../../../Protocol/Protocol.proto"; // 프로토파일 열기
            if (args.Length >= 1)
                file = args[0];

            bool startParsing = false;
            foreach (string line in File.ReadAllLines(file))
            {
                // 패킷ID 파싱 시작 지점 찾기
                if (!startParsing && line.Contains("enum MsgId"))   // 프로토 내부 enum msgid 찾기
                {
                    startParsing = true;
                    continue;
                }
                // 다음 줄 처리
                if (!startParsing)
                    continue;
                // 파싱 끝지점 찾으면 종료
                if (line.Contains("}"))
                    break;
                // 줄을 분할하여 패킷ID와 메시지 이름 추출
                string[] names = line.Trim().Split(" =");
                if (names.Length == 0)
                    continue;

                string name = names[0];
                if (name.StartsWith("S_"))  // S_ 시작 명령어 확인
                {
                    string[] words = name.Split("_");

                    string msgName = "";
                    foreach (string word in words)
                        msgName += FirstCharToUpper(word);

                    string packetName = $"S_{msgName.Substring(1)}";
                    // 클라이언트 레지스터에 등록
                    clientRegister += string.Format(PacketFormat.managerRegisterFormat, msgName, packetName);
                }
                else if (name.StartsWith("C_")) // C_ 시작 명령어
                {
                    string[] words = name.Split("_");

                    string msgName = "";
                    foreach (string word in words)
                        msgName += FirstCharToUpper(word);

                    string packetName = $"C_{msgName.Substring(1)}";
                    // 서버 레지스터에 등록
                    serverRegister += string.Format(PacketFormat.managerRegisterFormat, msgName, packetName);
                }
            }

            string clientManagerText = string.Format(PacketFormat.managerFormat, clientRegister);
            File.WriteAllText("ClientPacketManager.cs", clientManagerText);  // 클라이언트 패킷매니저
            string serverManagerText = string.Format(PacketFormat.managerFormat, serverRegister);
            File.WriteAllText("ServerPacketManager.cs", serverManagerText);  // 서버패킷매니저
        }

        public static string FirstCharToUpper(string input)
        {
            if (string.IsNullOrEmpty(input))
                return "";
            return input[0].ToString().ToUpper() + input.Substring(1).ToLower();
        }
    }
}
