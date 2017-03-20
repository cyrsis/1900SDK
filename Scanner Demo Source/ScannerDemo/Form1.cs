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

/*******************************************************************************************************
* This program illustrates two techniques that can be used to perform image capture from a scanner. The
* simplest method is to just make a blocking call to IMGSNP that will wait until an image is captured.
* However, this is problematic for two reasons - first, there is no preview so we cannot be sure exactly
* what the scanner has captured, second, IMGNSP will wait indefinitely for the trigger to be pressed so
* it is not possible to implement a timeout. An alternative approach is to place the scanner in a mode
* where it will send a message when the trigger is pulled, so the program waits for this message before
* sending the IMGSNP. By using this method, it is also possible to implement a reduced-size "streaming
* video" preview which can be used to aim the scanner. To capture the full-sized image in this mode,
* press and hold the trigger until the program beeps to indicate the image has been captured.
* 
* This code is UNSUPPORTED and is provided for demonstration purposes only.
*********************************************************************************************************/

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Media;
using System.Threading;
using System.Reflection;
using Honeywell.DemoCode;

namespace ScannerDemo
{
	public partial class Form1 : Form
	{
		private ScannerHandler Scanner;						// Scanner object that we will use to talk to the scanner with
		private List<Control> CList = new List<Control>();	// List of controls to be disabled when capturing images
		private string[] Instructions = new string[]
		{
			"Scan a barcode, or click Start button to capture an image",
			"Pull and hold trigger or click Capture button to capture an image",
			"Pull and release the trigger to capture an image"
		};

		// These fields are used for inter-thread communication, and so must be declared "volatile"
		private volatile bool Capturing = false;			// Current image capture state
		private volatile bool VideoCapture = false;			// Flag to tell video thread to capture final image
		private volatile bool VideoExit = false;			// Flag to force video thread to exit

		public Form1()
		{
			InitializeComponent();

			// Add controls to the list that we want to enable/disable together
			CList.Add(cbPreview);
			CList.Add(lbIllumination);
			CList.Add(cbIllumPreview);
			CList.Add(cbIllumCapture);
			CList.Add(btStart);
			CList.Add(tbResponse);
			// Set the initial instructions
			lbInstructions.Text = Instructions[0];
		}

		// Get the scanner port and try to open it
		private void Form1_Load(object sender, EventArgs e)
		{
			new FormGetPort().ShowDialog();
			if (FormGetPort.Cancel)
			{
				// The Cancel button was clicked, so just exit
				Close();
				return;
			}
			if (FormGetPort.Port == null)
			{
				// No serial ports were found
				MessageBox.Show("Could not find serial ports", "Scanner Demo", MessageBoxButtons.OK, MessageBoxIcon.Error);
				Close();
				return;
			}
			// Try to open the scanner
			try
			{
				Scanner = new ScannerHandler(FormGetPort.Port);
			}
			catch
			{
				// Open failed - show an error message an exit
				MessageBox.Show("Could not open scanner", "Scanner Demo", MessageBoxButtons.OK, MessageBoxIcon.Error);
				Close();
				return;
			}
			// Finally, check the scanner is responding. We will send the "DECHDR1" command which will make the scanner add a prefix to
			// any barcode data it has scanned. This lets barcode scanning and image capturing co-exist more easily as the scanner handler
			// can then tell easily whether data that arrives from the scanner is barcode scan data or not. The scanner will be set back
			// to the default mode "DECHDR0" when the form is closed.
			if (!Scanner.SendMenuCommand("DECHDR1", 1000))
			{
				MessageBox.Show("Scanner not responding", "Scanner Demo", MessageBoxButtons.OK, MessageBoxIcon.Error);
				Close();
				return;
			}
			// Set the barcode and unsolicited response event handlers
			Scanner.BarcodeScan += BarcodeScan;
			Scanner.UnsolicitedResponse += UnsolicitedResponse;
		}

		// Start the image capture. Image capture can be performed one of two ways:
		//
		// 1) If the "Show preview" checkbox is checked, we will start a thread that repeatedly sends snap and ship commands
		//    for a half-size image and displays the results. When the trigger is pulled, a final snap and ship is performed
		//    for a full-size image, and the thread then exits.
		//
		// 2) If the "Show preview" checkbox is not checked, image snap and ship commands will be sent immediately to the
		//	  scanner. These commands will block until the trigger is pulled and released.
		//
		private void btStart_Click(object sender, EventArgs e)
		{
			bool success;

			// Disable/hide/clear controls
			foreach (Control c in CList) c.Enabled = false;
			pbCapture.Image = null;
			btSave.Visible = false;
			tbResponse.Text = "";
			// Will a live preview be shown during the image capture?
			if (cbPreview.Checked)
			{
				// Yes - set up the trigger callback and change to trigger mode 5 so we get trigger notification messages
				Scanner.TriggerPull += TriggerPull;
				Scanner.SendMenuCommand("TRGMOD5");
				// Enable the Capture button so we can capture an image immediately
				btCapture.Enabled = true;
				// Enable the Stop button so we can stop the preview without capturing an image
				btStop.Enabled = true;
				btStop.Focus();
				// Prevent form from closing
				Capturing = true;
				// Show instructions
				lbInstructions.Text = Instructions[1];
				// Create and start the video handler thread with a 10 second timeout
				new Thread(VideoHandler).Start(10000);
			}
			else
			{
				// Show instructions and ensure the form is updated
				lbInstructions.Text = Instructions[2];
				Application.DoEvents();
				// Ensure scanner is in default trigger state
				Scanner.SendMenuCommand("TRGMOD0");
				// Snap and ship the image. Note that JPG format ("6F") MUST be selected - this is done to reduce memory
				// utilization, reduce the amount of data sent over the interface, and to avoid serious know issues with
				// GDI+. See the ScannerHandler class for more information.
				success = Scanner.SendMenuCommand("IMGSNP" + (cbIllumCapture.Checked ? "1" : "0") + "L1T;" +
												  "IMGSHP2P6F");
				// If the capture succeeded then update the picture box 
				if (success)
				{
					pbCapture.Image = Scanner.LastImage;
				}
				CaptureComplete(success, true);
			}
		}

		// Force an image capture without a trigger pull
		private void btCapture_Click(object sender, EventArgs e)
		{
			// Set the flag so that a final image will be captured on the next iteration
			VideoCapture = true;
		}

		// Stop the image capture
		private void btStop_Click(object sender, EventArgs e)
		{
			// Tell the video preview thread to exit
			VideoExit = true;
		}

		// Save the image
		private void btSave_Click(object sender, EventArgs e)
		{
			// Display a file save dialog
			SaveFileDialog saveFileDialog1 = new SaveFileDialog();
			saveFileDialog1.Filter = "JPEG Image|*.jpg|Bitmap Image|*.bmp|PNG Image|*.png";
			saveFileDialog1.Title = "Save Image to File";
			saveFileDialog1.ShowDialog();

			// If the file name is not an empty string, save the file
			if (saveFileDialog1.FileName != "")
			{
				// Set the image format
				ImageFormat format = ImageFormat.Jpeg;
				switch (saveFileDialog1.FilterIndex)
				{
					case 1:
						format = ImageFormat.Jpeg;
						break;
					case 2:
						format = ImageFormat.Bmp;
						break;
					case 3:
						format = ImageFormat.Png;
						break;
				}
				try
				{
					// Save the file
					pbCapture.Image.Save(saveFileDialog1.FileName, format);
				}
				catch
				{
					// File save failed, so show an error message
					MessageBox.Show("File save failed", "Scanner Demo", MessageBoxButtons.OK, MessageBoxIcon.Error);
				}
			}
			// Set focus back to the Start button
			btStart.Focus();
		}

		// Preview mode changed
		private void cbPreview_CheckedChanged(object sender, EventArgs e)
		{
			// Preview illumination check box will only be visible when preview is enabled
			cbIllumPreview.Visible = cbPreview.Checked;
		}

		// Preview illumination changed
		private void cbIllumPreview_CheckedChanged(object sender, EventArgs e)
		{
			// If preview illumination is now checked, check capture illumination as well
			if (cbIllumPreview.Checked)
			{
				cbIllumCapture.Checked = true;
			}
		}

		// Capture illumination changed
		private void cbIllumCapture_CheckedChanged(object sender, EventArgs e)
		{
			// If capture illumination is now not checked, uncheck preview illumination
			if (!cbIllumCapture.Checked)
			{
				cbIllumPreview.Checked = false;
			}
		}

		// Tidy up before exiting. We will only allow the form to close if we are not actually doing an
		// image capture. It would be possible to allow the close when an image capture was active, but
		// it would be quite complex as it would need to avoid a deadlock between the video preview thread
		// and the main UI thread.
		private void Form1_FormClosing(object sender, FormClosingEventArgs e)
		{
			// See if we are allowed to close the form
			if (Capturing)
			{
				// No - play a beep sound and don't allow the form to close
				System.Media.SystemSounds.Beep.Play();
				e.Cancel = true;
			}
			else if (Scanner != null)
			{
				// Reset the scanner mode, remove the event handlers and allow the form to close
				Scanner.SendMenuCommand("TRGMOD0;DECHDR0", 1000);
				Scanner.BarcodeScan -= BarcodeScan;
				Scanner.UnsolicitedResponse -= UnsolicitedResponse;
				Scanner.Close();
			}
		}

		// This will be called when the capture operation has completed
		private void CaptureComplete(bool success, bool playsound)
		{
			// Play success or failure sounds if required
			if (playsound)
			{
				if (success)
				{
					// Play a wave file from an embedded resource
					Assembly xa = Assembly.GetExecutingAssembly();
					new SoundPlayer(xa.GetManifestResourceStream(xa.GetName().Name + ".beep.wav")).Play();
				}
				else
				{
					System.Media.SystemSounds.Beep.Play();
				}
			}
			// Return the scanner to the default trigger state
			Scanner.SendMenuCommand("TRGMOD0");
			// If non-blocking mode, remove the event handler
			if (cbPreview.Checked)
			{
				Scanner.TriggerPull -= TriggerPull;
			}
			// Reset the form controls for the next capture - use Invoke in case this is not called from the UI thread
			Invoke((MethodInvoker)delegate
			{
				if (success)
				{
					// An image was captured, so make the Save button visible
					btSave.Visible = true;
				}
				else
				{
					// No image was captured, so clear any preview image
					pbCapture.Image = null;
					btSave.Visible = false;
				}
				foreach (Control c in CList) c.Enabled = true;
				lbInstructions.Text = Instructions[0];
				btCapture.Enabled = false;
				btStop.Enabled = false;
				btStart.Focus();
			});
			// Flag the capture as complete so the form can be closed
			Capturing = false;
		}

		// Barcode scan event
		private void BarcodeScan(object sender, ScannerHandler.BarcodeScanEventArgs e)
		{
			// Update the text box and clear the picture box - use Invoke in case this is not called from the UI thread
			Invoke((MethodInvoker)delegate
			{
				tbResponse.Text = e.BarcodeData;
				pbCapture.Image = null;
				btSave.Visible = false;
			});
		}
	
		// Trigger pull event
		private void TriggerPull(object sender, EventArgs e)
		{
			// Set the flag so that a final image will be captured on the next iteration
			VideoCapture = true;
		}

		// Unsolicited response event
		//
		// This event will be fired when data other than a barcode or an image is received, typically in response to scanning a command
		// barcode (or sending a command) that returns information about a cordless scanner. The event is not normally required as in
		// general it is not possible to determine what sort of data the response contains. However, it will be needed if any commands
		// such as "BT_LDA" or "RPTSCN" are sent as the responses will be delivered via this event and not as part of the command response
		// from the ScannerHandler.SendMenuCommand method.  Note that for all unsolicited responses the final CRLF is removed, and for
		// "RPTSCAN", the intial CRLF is also removed.
		private void UnsolicitedResponse(object sender, ScannerHandler.UnsolicitedResponseEventArgs e)
		{
			// Update the text box and clear the picture box - use Invoke in case this is not called from the UI thread
			Invoke((MethodInvoker)delegate
			{
				tbResponse.Text = e.Response;
				pbCapture.Image = null;
				btSave.Visible = false;
			});
		}

		// Video preview thread
		//
		// If using the non-blocking method, this thread is used to capture and display images. The parameter
		// is a boxed int holding a timeout value in milliseconds
		private void VideoHandler(object Timeout)
		{
			bool success = false;
			
			// Set flags to show the thread is running
			VideoExit = false;
			// Set the capture flag to capture the final image immediately if the timeout was 0
			VideoCapture = false;
			// Set up a stopwatch to monitor the timeout
			System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
			stopwatch.Start();
			// Loop until the VideoExit flag becomes set
			while (!VideoExit)
			{
				// Take a temporary copy of the video capture flag to avoid a potential race condition if the value
				// of the flag changes while a snap/ship is actually taking place
				bool videocapture = VideoCapture;
				// Snap and ship the image. Note that JPG format ("6F") MUST be selected - this is done to reduce memory
				// utilization, reduce the amount of data sent over the interface, and to avoid serious know issues with
				// GDI+. See the ScannerHandler class for more information.
				success = Scanner.SendMenuCommand("IMGSNP" + ((videocapture ? cbIllumCapture.Checked : cbIllumPreview.Checked) ? "1" : "0") + "L;" +
												  "IMGSHP2P6F" + (videocapture ? "1" : "2") + "S");
				// If the capture succeed then update the picture box - use Invoke in case this is not called from the UI thread
				if (success)
				{
					Invoke((MethodInvoker)delegate { pbCapture.Image = Scanner.LastImage; });
				}
				// If this was not the final image capture then clear the success flag in case we are going to exit				
				if (!videocapture)
				{
					success = false;
				}
				// If this was the final image capture or if the timeout has expired then exit
				if (videocapture || stopwatch.ElapsedMilliseconds > (int)Timeout)
				{
					break;
				}
			}
			// Call the capture completion method to reset the form
			CaptureComplete(success, !VideoExit);
		}
	}
}
