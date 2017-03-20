/****************************************************************************************************

THIS SOFTWARE IS PROVIDED "AS IS" AND, UNLESS OTHERWISE EXPRESSLY PROVIDED HEREIN OR EXCEPT AS
REQUIRED BY LAW, HONEYWELL AND ITS LICENSORS AND SUPPLIERS EACH DISCLAIM ALL WARRANTIES, EXPRESS
OR IMPLIED, INCLUDING, BUT NOT LIMITED TO, IMPLIED WARRANTIES OF TITLE, MERCHANTABILITY, FITNESS
FOR A PARTICULAR PURPOSE, TRADE USAGE, COURSE OF DEALING,AND NON-INFRINGEMENT WITH RESPECT TO SUCH
SOFTWARE.

IN NO EVENT SHALL HONEYWELL BE RESPONSIBLE OR LIABLE FOR ANY INDIRECT, EXEMPLARY, INCIDENTAL,
SPECIAL, PUNITIVE, RELIANCE OR CONSEQUENTIAL DAMAGES, OR FOR LOSS OF PROFITS, EVEN IF HONEYWELL
HAS BEEN PREVIOUSLY ADVISED OF THE POSSIBILITY OF SUCH DAMAGES, WHETHER IN AN ACTION UNDER
CONTRACT, TORT, STRICT LIABILITY, OR OTHER THEORY, ARISING OUT OF OR IN CONNECTION WITH THE USE
OR PERFORMANCE OF THIS SOFTWARE.

Copyright (c) 2011 HONEYWELL INTERNATIONAL INC.

****************************************************************************************************/

/**********************************************************************************************************
* This component interfaces to the scanner. It opens the scanner's serial port when it is constructed,
* and closes it when it is disposed. An explicit Close method is also provided. It handles serial input,
* and responds to both synchronous and asynchronous responses from the scanner.
* 
* This code is UNSUPPORTED and is provided for demonstration purposes only. It does not handle all of the
* possible error conditions that could be encountered in a production environment.
**********************************************************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.IO.Ports;
using System.Drawing;
using System.Threading;
using System.ComponentModel;

namespace Honeywell.DemoCode
{
	public class ScannerHandler : Component
	{
		#region Private Fields

		private SerialPort port = null;				// Serial port
		private Thread listener;					// Listener thread
		private bool disposed = false;				// Flag to detect redundant disposes
		private AutoResetEvent eresponse;			// Event to wait for command response
		private string sresponse = "";				// Command response
		private System.Drawing.Image image;			// Last image captured

		// These fields are used for inter-thread communication, and so must be declared "volatile"
		private volatile bool exitthread = false;	// Flag used to signal serial port thread to exit
		private volatile bool success = false;		// Flag used to signal command success/failure
		private volatile int responsecount = 0;		// Count of command responses being waited for

		// Receive buffer - must be big enough to hold the largest possible image
		private static byte[] buffer = new byte[600000];

		#endregion

		#region Private Constants

		// Constants
		private const char ACK = '\x06';			// Command response was OK
		private const char ENQ = '\x05';			// Command response was not OK
		private const char NAK = '\x15';			// Command response was not OK
		private const char SYN = '\x16';			// Command prefix and asynchronous message indicator

		#endregion

		#region Public Classes

		/// <summary>
		/// Unsolicited response event arguments
		/// </summary>
		public class UnsolicitedResponseEventArgs : EventArgs
		{
			private string response;

			/// <summary>
			/// Barcode scan event arguments
			/// </summary>
			/// <param name="Response">Response text</param>
			public UnsolicitedResponseEventArgs(string Response)
				: base()
			{
				response = Response;
			}

			/// <summary>
			/// Response test
			/// </summary>
			public string Response
			{
				get { return response; }
			}
		}

		/// <summary>
		/// Barcode scan event arguments
		/// </summary>
		public class BarcodeScanEventArgs : EventArgs
		{
			private char aimid;
			private char aimmod;
			private string barcodedata;

			/// <summary>
			/// Barcode scan event arguments
			/// </summary>
			/// <param name="AIMID">AIM ID</param>
			/// <param name="AIMMod">AIM modifier</param>
			/// <param name="BarcodeData">Barcode data</param>
			public BarcodeScanEventArgs(char AIMID, char AIMMod, string BarcodeData)
				: base()
			{
				aimid = AIMID;
				aimmod = AIMMod;
				barcodedata = BarcodeData;
			}

			/// <summary>
			/// AIM ID
			/// </summary>
			public char AIMID
			{
				get { return aimid; }
			}

			/// <summary>
			/// AIM modifier
			/// </summary>
			public char AIMMod
			{
				get { return aimmod; }
			}

			/// <summary>
			/// Barcode data
			/// </summary>
			public string BarcodeData
			{
				get { return barcodedata; }
			}
		}

		#endregion

		#region Public Delegates

		// Delegates for events
		public delegate void TriggerPullEventHandler(object sender, EventArgs e);
		public delegate void BarcodeScanEventHandler(object sender, BarcodeScanEventArgs e);
		public delegate void UnsolicitedResponseEventHandler(object sender, UnsolicitedResponseEventArgs e);

		#endregion

		#region Public Events

		/// <summary>
		/// Occurs when the scanner's trigger is pulled
		/// </summary>
		public event TriggerPullEventHandler TriggerPull;

		/// <summary>
		/// Occurs when a barcode is scanned
		/// </summary>
		public event BarcodeScanEventHandler BarcodeScan;

		/// <summary>
		/// Occurs when data other than a barcode scan or an image arrives from the scanner
		/// </summary>
		public event UnsolicitedResponseEventHandler UnsolicitedResponse;

		#endregion

		#region Public Properties

		/// <summary>
		/// Scanner open state
		/// </summary>
		public bool IsOpen
		{
			get
			{
				return port != null && port.IsOpen;
			}
		}

		/// <summary>
		/// Most recently captured image
		/// </summary>
		public Image LastImage
		{
			get
			{
				return image;
			}
		}

		#endregion

		#region Constructor

		/// <summary>
		/// Create a serial port handler
		/// </summary>
		public ScannerHandler(string PortName)
		{
			port = new SerialPort(PortName, 115200);	// Create a new serial port object
			port.ReadBufferSize = 50000;				// Set a very large buffer size
			port.Open();								// Open the port
			port.DiscardInBuffer();						// Remove any data remaining in the scanner
			eresponse = new AutoResetEvent(false);		// Create an event to wait on
			listener = new Thread(ReceiveHandler);		// Create the serial port listener thread
			listener.Start();							// Start the thread
		}

		#endregion

		#region Public Methods

		/// <summary>
		/// Close the serial port
		/// </summary>
		public void Close()
		{
			if (IsOpen)
			{
				exitthread = true;		// Tell listener thread to exit
				listener.Join(1000);	// Wait for it to exit for up to 1 second
				port.Close();			// Close the serial port
				port.Dispose();			// Dispose it
				port = null;
			}
		}

		// SendMenuCommand
		//
		// This method sends a command to the scanner. Most commands will return their response in the optional Response
		// parameter, but note that the responses from certain commands (or their corresponding command barcodes) that return
		// information about cordless scanners, such as "BT_LDA" or "RPTSCN", will be sent via the UnsolicitedResponse event
		// instead. In this case the final CRLF of the response is removed, and for "RPTSCAN", the intial CRLF is also removed.
		
		/// <summary>
		/// Send a menu command to the scanner
		/// </summary>
		/// <param name="Command">Command string to send</param>
		/// <returns>True if command succeeded</returns>
		public bool SendMenuCommand(string Command)
		{
			string temp;
			return SendMenuCommand(Command, out temp, 0, false);
		}

		/// <summary>
		/// Send a menu command to the scanner and get the response
		/// </summary>
		/// <param name="Command">Command string to send</param>
		/// <param name="Response">Command response</param>
		/// <returns>True if command succeeded</returns>
		public bool SendMenuCommand(string Command, out string Response)
		{
			return SendMenuCommand(Command, out Response, 0, false);
		}

		/// <summary>
		/// Send a menu command to the scanner and get the response with a timeout
		/// </summary>
		/// <param name="Command">Command string to send</param>
		/// <param name="Timeoutms">Timeout in milliseconds or 0 to wait indefinitely</param>
		/// <returns>True if command succeeded</returns>
		public bool SendMenuCommand(string Command, int Timeoutms)
		{
			string temp;
			return SendMenuCommand(Command, out temp, Timeoutms, false);
		}

		/// <summary>
		/// Send a menu command to the scanner and get the response with a timeout
		/// </summary>
		/// <param name="Command">Command string to send</param>
		/// <param name="Response">Command response</param>
		/// <param name="Timeoutms">Timeout in milliseconds or 0 to wait indefinitely</param>
		/// <returns>True if command succeeded</returns>
		public bool SendMenuCommand(string Command, out string Response, int Timeoutms)
		{
			return SendMenuCommand(Command, out Response, Timeoutms, false);
		}

		/// <summary>
		/// Send a menu command to the scanner and get the response with a timeout
		/// </summary>
		/// <param name="Command">Command string to send</param>
		/// <param name="Response">Command response</param>
		/// <param name="Timeoutms">Timeout in milliseconds or 0 to wait indefinitely</param>
		/// <param name="CommitToFlash">Set true to commit programming command to FLASH memory</param>
		/// <returns>True if command succeeded</returns>
		public bool SendMenuCommand(string Command, out string Response, int Timeoutms, bool CommitToFLASH)
		{
			Response = "";
			if (IsOpen)
			{
				responsecount = Command.Split(new char[] { ';' }).Length ;						// Set response count to number of commands
				success = false;			// Set initial state
				eresponse.Reset();			// Clear event
				port.Write(SYN.ToString() + "M\r" + Command + (CommitToFLASH ? "." : "!"));		// Copy the command to the port
				bool state = eresponse.WaitOne(Timeoutms > 0 ? Timeoutms : Timeout.Infinite);	// Wait for the response
				responsecount = 0;			// In case we timed out, tell listener thread we are no longer waiting
				Response = sresponse;		// Set response string
				return state && success;	// Return state
			}
			return false;
		}

		/// <summary>
		/// Close the serial port and Dispose the class
		/// </summary>
		public new void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		/// <summary>
		/// Close the serial port and Dispose the class
		/// </summary>
		/// <param name="disposing">Disposing flag</param>
		protected override void Dispose(bool disposing)
		{
			if (!disposed)
			{
				if (disposing)
				{
					// Dispose any managed resources here
					Close();
				}
				// Release any unmanaged resources here
			}
			disposed = true;
			base.Dispose(disposing);
		}

		#endregion

		#region Private Methods
		
		// Serial port listener thread
		//
		// This thread processes all input from the serial port. Input will be of 2 types:
		//	- Synchronous responses to commands that have been sent to the scanner
		//	- Asynchronous responses to events generated by the scanner
		private void ReceiveHandler()
		{
			int count = 0;			// Number of bytes in buffer
			bool uappend = false;	// Unsolicited response append flag
			string lresponse = "";	// String to accumulate command responses in
			string uresponse = "";	// String to accumulate unsolicited responses in

			// Check each time round whether we need to exit
			while (!exitthread)
			{
				// See if there is anything left in the buffer or any more bytes to be read from the port
				if (count > 0 || port.BytesToRead > 0)
				{
					// Yes - if there are more bytes to read, get them and increment the current count
					if (port.BytesToRead > 0)
					{
						count += port.Read(buffer, count, buffer.Length - count);
					}
					// See whether this is a synchronous or asynchronous response
					if (count > 0 && buffer[0] == SYN)
					{
						// This is an asynchronous response, in this case either a trigger notification or image data
						// First, see if the header has arrived completely
						if (count >= 7)
						{
							// Yes, so get the message length
							int len = (((((buffer[5] << 8) + buffer[4]) << 8) + buffer[3]) << 8) + buffer[2] + 7;
							// See if we have got the complete message
							if (count >= len)
							{
								// Yes, so see what type of message it is
								switch (Encoding.Default.GetString(buffer, 7, 6))
								{
									case "MSGGET":
										// Barcode scan - we only need to do something if the barcode event has a handler
										if (BarcodeScan != null)
										{
											// Find the start of the barcode data
											for (int i = 13; i < len; i++)
											{
												if (buffer[i] == '\x1d')
												{
													// Call the barcode event handler, passing the AIM ID, AIM modifier and the barcode data
													BarcodeScan(this, new BarcodeScanEventArgs((char)buffer[i - 2], (char)buffer[i - 1], Encoding.Default.GetString(buffer, i + 1, len - i - 1)));
													break;
												}
											}
										}
										break;
									case "TRGEVT":
										// Trigger event - we only need to do something if the trigger event has a handler
										if (TriggerPull != null)
										{
											// For forward compatibility, find and check the trigger state flag
											for (int i = 13; i < len; i++)
											{
												if (buffer[i] == 'T' && buffer[i - 1] != '0')
												{
													// The trigger has been pressed, so call the trigger event handler
													TriggerPull(this, new EventArgs());
													break;
												}
											}
										}
										break;
									case "IMGSHP":
										image = null;
										// Find the start of the image data
										for (int i = 13; i < len; i++)
										{
											if (buffer[i] == '\x1d')
											{
												//*******************************************************************************
												//*                                  IMPORTANT!                                 *
												//*******************************************************************************
												// Now create a managed image object from the byte data. Due to issues with GDI+
												// and the way it handles memory streams, care must be taken with this process.
												// To avoid the most serious of these issues, a "generic error" exception that
												// GDI+ can throw when creating images/bitmaps from memory streams, it would be
												// possible to write the image data to a file and then load it back into an image
												// object. However, this is the wrong way around as at this stage we don't know if
												// the image needs to be written or just displayed. The issue will typically show
												// when attempting to create images from BMP data rather than JPG, so the data MUST
												// always be in JPG format. Note that a managed bitmap is created as an intermediate
												// step. This makes things slightly slower, but avoids issues later on because if
												// an image is created directly from a MemoryStream then the stream must remain
												// valid for the life of the image.
												//*******************************************************************************
												image = Bitmap.FromStream(new MemoryStream(buffer, i + 1, len - i - 1));
												break;
											}
										}
										break;
								}
								// Remove message from buffer and shift the remaining bytes up, if any
								Array.Copy(buffer, len, buffer, 0, count - len);
								count -= len;
							}
						}
					}
					else
					{
						// This is either a synchronous command response, typically the original command echoed back with an ACK,
						// NAK or ENQ appended, or an unsolicited text response caused by scanning a cordless query-type barcode
						// such as the one used to return the scanner address
						success = true;
						for (int i = 1; i < count; i++)
						{
							// Look for the command terminator character
							if (buffer[i] == ';' || buffer[i] == '.' || buffer[i] == '!')
							{
								// Might have found it - check for the command result
								if (buffer[i - 1] == NAK || buffer[i - 1] == ENQ)
								{
									// The command failed
									success = false;
								}
								else if (buffer[i - 1] != ACK)
								{
									// The character was not ACK, NAK or ENQ - probably a terminator character was found
									// at some place other than the end of the command response, so keep on looking
									continue;
								}
								// This is a definitely a command response, so see if the command thread is still waiting for a response
								if (responsecount > 0)
								{
									// Yes - convert the command result to a string and add to the current response
									lresponse += Encoding.Default.GetString(buffer, 0, i);
									// See if we have had all the responses we are expecting
									if (--responsecount == 0)
									{
										// Yes - set final response string and clear temporary one
										sresponse = lresponse;
										lresponse = "";
										// Release the command thread
										eresponse.Set();
									}
								}
								else
								{
									// No - the command must have timed out, so reset the temporary response buffer in
									// case it already had something in it
									sresponse = "";
								}
								// Remove response from buffer and shift other bytes up
								Array.Copy(buffer, i + 1, buffer, 0, count - i - 1);
								count -= i + 1;
								break;
							}
							else if (buffer[i-1] == '\x0d' && buffer[i] == '\x0a')
							{
								// This is an unsolicited text response, but we need only do something if the unsolicited response
								// event has a handler
								if (UnsolicitedResponse != null)
								{
									// See if this is the first part of an RPTSCN response - this starts with a CRLF, and then contains
									// 2 lines of data each terminated by a CRLF
									if (i == 1)
									{
										// Yes - set the unsolicited message append flag so we will accumulate the next 2 messages
										uappend = true;
									}
									else if (uappend)
									{
										uresponse = Encoding.Default.GetString(buffer, 0, i + 1);
										uappend = false;
									}
									else
									{
										// Call the event handler and pass in the text response without the CRLF
										UnsolicitedResponse(this, new UnsolicitedResponseEventArgs(uresponse + Encoding.Default.GetString(buffer, 0, i - 1)));
										uresponse = "";
									}
								}
								// Remove response from buffer and shift other bytes up
								Array.Copy(buffer, i + 1, buffer, 0, count - i - 1);
								count -= i + 1;
							}
						}
					}
				}
				else
				{
					// Nothing to do, so wait 1 millisecond
					Thread.Sleep(1);
				}
			}
			// If we get here then the thread is exiting, so check if another thread is still waiting on a response
			if (responsecount > 0)
			{
				// Yes - set result to false and release thread
				sresponse = "";
				success = false;
				eresponse.Set();
			}
		}

		#endregion
	}
}
