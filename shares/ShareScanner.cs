﻿//
// Copyright (c) Ping Castle. All rights reserved.
// https://www.pingcastle.com
//
// Licensed under the Non-Profit OSL. See LICENSE file in the project root for full license information.
//
using PingCastle.ADWS;
using PingCastle.misc;
using PingCastle.Scanners;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Security.Principal;
using System.Text;
using System.Threading;

namespace PingCastle.shares
{
	public class ShareScanner : ScannerBase
    {
		public override string Name { get { return "share"; } }
		public override string Description { get { return "List all shares published on a computer and determine if the share can be accessed by anyone"; } }

		override protected string GetCsvHeader()
		{
			return "Computer\tShare\tIsEveryoneAllowed";
		}

		override protected string GetCsvData(string computer)
		{
			string output = null;
			if (IsServerAvailable(computer))
			{
				foreach (string path in ShareEnumerator.EnumShare(computer))
				{
					bool everyone = ShareEnumerator.IsEveryoneAllowed(computer, path);
					if (!String.IsNullOrEmpty(output))
						output += "\r\n";
					output += computer + "\t" + path + "\t" + everyone;
				}
			}
			return output;
        }

        const int timeout = 2;
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2202:Ne pas supprimer d'objets plusieurs fois")]
        public static bool IsServerAvailable(string ServerName)
        {
            var result = false;
            using (var client = new TcpClient())
            {
                try
                {
                    client.ReceiveTimeout = timeout * 1000;
                    client.SendTimeout = timeout * 1000;
                    var asyncResult = client.BeginConnect(ServerName, 445, null, null);
                    var waitHandle = asyncResult.AsyncWaitHandle;
                    try
                    {
                        if (!asyncResult.AsyncWaitHandle.WaitOne(TimeSpan.FromSeconds(timeout), false))
                        {
                            // wait handle didn't came back in time
                            client.Close();
                        }
                        else
                        {
                            // The result was positiv
                            result = client.Connected;
                        }
                        // ensure the ending-call
                        client.EndConnect(asyncResult);
                    }
                    finally
                    {
                        // Ensure to close the wait handle.
                        waitHandle.Close();
                    }
                }
                catch
                {
                }
            }
            return result;
        } 
    }
}