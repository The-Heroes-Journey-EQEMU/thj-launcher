using System;
using System.Windows;
using System.Windows.Controls;
using System.Diagnostics;

namespace THJPatcher.Views
{
    public partial class InstallationWizard : UserControl
    {
        public event EventHandler StartInstallationRequested;
        private int currentStep = 1;
        private const int TOTAL_STEPS = 9;

        public InstallationWizard()
        {
            InitializeComponent();
            HideNavigationButtons();
            ShowStep(1);
        }

        private void ShowStep(int stepNumber)
        {
            Debug.WriteLine($"Attempting to show step {stepNumber}");
            
            // Hide all steps first
            for (int i = 1; i <= TOTAL_STEPS; i++)
            {
                var step = FindName($"Step{i}") as Grid;
                if (step != null)
                {
                    step.Visibility = Visibility.Collapsed;
                    Debug.WriteLine($"Hidden Step{i}");
                }
            }

            // Show the current step
            var currentStep = FindName($"Step{stepNumber}") as Grid;
            if (currentStep != null)
            {
                currentStep.Visibility = Visibility.Visible;
                Debug.WriteLine($"Showed Step{stepNumber}");
            }
            else
            {
                Debug.WriteLine($"Failed to find Step{stepNumber}");
            }
        }

        public void ShowNavigationButtons()
        {
            if (PreviousButton != null && NextButton != null)
            {
                PreviousButton.Visibility = Visibility.Visible;
                NextButton.Visibility = Visibility.Visible;
            }
        }

        public void HideNavigationButtons()
        {
            if (PreviousButton != null && NextButton != null)
            {
                PreviousButton.Visibility = Visibility.Collapsed;
                NextButton.Visibility = Visibility.Collapsed;
            }
        }

        private void NextButton_Click(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine($"Next button clicked at step {currentStep}");
            
            // Remove any existing event handlers
            NextButton.Click -= NextButton_Click;
            NextButton.Click -= StartInstallation;
            
            if (currentStep < TOTAL_STEPS)
            {
                currentStep++;
                ShowStep(currentStep);
                
                // Update button state
                UpdateNavigationButtons();
            }
            
            Debug.WriteLine($"Now at step {currentStep}");
        }

        private void PreviousButton_Click(object sender, RoutedEventArgs e)
        {
            if (currentStep > 1)
            {
                currentStep--;
                ShowStep(currentStep);
                UpdateNavigationButtons();
                Debug.WriteLine($"Moved to previous step: {currentStep}");
            }
        }

        private void UpdateNavigationButtons()
        {
            if (PreviousButton != null && NextButton != null)
            {
                PreviousButton.IsEnabled = currentStep > 1;
                
                // Clear existing handlers
                NextButton.Click -= NextButton_Click;
                NextButton.Click -= StartInstallation;
                
                if (currentStep >= TOTAL_STEPS)
                {
                    NextButton.Content = "Start Installation";
                    NextButton.Click += StartInstallation;
                }
                else
                {
                    NextButton.Content = "Next";
                    NextButton.Click += NextButton_Click;
                }
            }
        }

        private void StartInstallation(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("Installation requested");
            
            try
            {
                StartInstallationRequested?.Invoke(this, EventArgs.Empty);
                Debug.WriteLine("Installation event successfully triggered");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error starting installation: {ex.Message}");
            }
        }
    }
}
