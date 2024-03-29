﻿using Microsoft.Office.Interop.Word;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.InputFiles;
using TelegramBotConverter.Classes;
using Xceed.Words.NET;

namespace TelegramBotConverter
{
    class Program
    {
        private static readonly TelegramBotClient bot = new TelegramBotClient("");
        static void Main(string[] args)
        {
            bot.OnMessage += Bot_OnMessage;
            bot.StartReceiving();
            Console.ReadKey();
            bot.StopReceiving();
        }

        private static void Bot_OnMessage(object sender, Telegram.Bot.Args.MessageEventArgs e)
        {
            var chatId = e.Message.Chat.Id;
            try
            {
                if (e.Message.Type == Telegram.Bot.Types.Enums.MessageType.Document)
                {
                    var document = e.Message.Document;

                    if (document.FileName.Contains(".docx") || document.FileName.Contains(".doc"))
                    {
                        bot.SendTextMessageAsync(e.Message.Chat.Id, "Жди!Обрабатываю!...");
                        ProgressFile(document, chatId);
                    }
                    else
                    {
                        throw new AppException("Расширение файла должно быть .docx или .doc");
                    }
                }
            }
            catch (AppException exeption)
            {
                bot.SendTextMessageAsync(chatId, exeption.Message);
            }
            catch (Exception)
            {
                ServerErrorMessage(chatId);
            }
        }

        private static async void ProgressFile(Telegram.Bot.Types.Document document, long chatId)
        {
            try
            {
                //save doc||docx file
                string newName = document.FileId + GetExtentionFile(document.FileName);
                using (var saveFileStream = new FileStream("FilesDownload\\" + newName, FileMode.Create))
                {
                    await bot.GetInfoAndDownloadFileAsync(document.FileId, saveFileStream);
                }

                CreatePdfFile(newName);

                using (FileStream fs = System.IO.File.OpenRead("FilesDownload\\" + document.FileId + ".pdf")) 
                {
                    InputOnlineFile inputOnlineFile = new InputOnlineFile(fs, document.FileId + ".pdf");
                    // Message message = bot.SendDocumentAsync(chatId, inputOnlineFile).Result;
                   await bot.SendDocumentAsync(chatId, inputOnlineFile);
                }
            }
            catch (Exception)
            {
                ServerErrorMessage(chatId);
            }
        }

        private static string GetExtentionFile(string fileName)
        {
            return "." + fileName.Split('.').Last();
        }

        public static void CreatePdfFile(string fileName)
        {
            Microsoft.Office.Interop.Word.Application word = new Microsoft.Office.Interop.Word.Application();

            // C# doesn't have optional arguments so we'll need a dummy value
            object oMissing = System.Reflection.Missing.Value;

            // Get list of Word files in specified directory
            // DirectoryInfo dirInfo = new DirectoryInfo("FilesDownload\\"/* @".\"*/);
            //  FileInfo[] wordFiles = dirInfo.GetFiles("*.docx");

            word.Visible = false;
            word.ScreenUpdating = false;
            FileInfo wordFile = new FileInfo("FilesDownload\\" + fileName);
            //foreach (FileInfo wordFile in wordFiles)
            // {
            // Cast as Object for word Open method
            Object filename = (Object)wordFile.FullName;

            // Use the dummy value as a placeholder for optional arguments
            Microsoft.Office.Interop.Word.Document doc = word.Documents.Open(ref filename, ref oMissing,
                ref oMissing, ref oMissing, ref oMissing, ref oMissing, ref oMissing,
                ref oMissing, ref oMissing, ref oMissing, ref oMissing, ref oMissing,
                ref oMissing, ref oMissing, ref oMissing, ref oMissing);
            doc.Activate();

            object outputFileName = wordFile.FullName.Replace(GetExtentionFile(fileName)/*".docx"*/, ".pdf");
            object fileFormat = WdSaveFormat.wdFormatPDF;

            // Save document into PDF Format
            doc.SaveAs(ref outputFileName,
                ref fileFormat, ref oMissing, ref oMissing,
                ref oMissing, ref oMissing, ref oMissing, ref oMissing,
                ref oMissing, ref oMissing, ref oMissing, ref oMissing,
                ref oMissing, ref oMissing, ref oMissing, ref oMissing);

            // Close the Word document, but leave the Word application open.
            // doc has to be cast to type _Document so that it will find the
            // correct Close method.                
            object saveChanges = WdSaveOptions.wdDoNotSaveChanges;
            ((_Document)doc).Close(ref saveChanges, ref oMissing, ref oMissing);
            doc = null;
            // }

            // word has to be cast to type _Application so that it will find
            // the correct Quit method.
            ((Application)word).Quit(ref oMissing, ref oMissing, ref oMissing);
        }

        private static async void ServerErrorMessage(long chatId)
        {
            await bot.SendTextMessageAsync(chatId, "Server Error!...");
        }
    }
}