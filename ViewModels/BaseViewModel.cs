using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System;

namespace JigaMultiplatform.ViewModels;

public abstract class BaseViewModel : INotifyPropertyChanged
{
    private bool _isBusy;
    private string _title = string.Empty;
    private string _subtitle = string.Empty;

    public bool IsBusy
    {
        get => _isBusy;
        set
        {
            if (SetProperty(ref _isBusy, value))
            {
                OnPropertyChanged(nameof(IsNotBusy));
            }
        }
    }

    public bool IsNotBusy => !IsBusy;

    public string Title
    {
        get => _title;
        set => SetProperty(ref _title, value);
    }

    public string Subtitle
    {
        get => _subtitle;
        set => SetProperty(ref _subtitle, value);
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    protected bool SetProperty<T>(ref T backingStore, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(backingStore, value))
            return false;

        backingStore = value;
        OnPropertyChanged(propertyName);
        return true;
    }

    protected virtual async Task ExecuteSafe(Func<Task> operation, string? loadingMessage = null)
    {
        try
        {
            IsBusy = true;
            
            if (!string.IsNullOrEmpty(loadingMessage))
                Subtitle = loadingMessage;

            await operation();
        }
        catch (Exception ex)
        {
            await HandleError(ex);
        }
        finally
        {
            IsBusy = false;
            Subtitle = string.Empty;
        }
    }

    protected virtual async Task<T?> ExecuteSafe<T>(Func<Task<T>> operation, string? loadingMessage = null)
    {
        try
        {
            IsBusy = true;
            
            if (!string.IsNullOrEmpty(loadingMessage))
                Subtitle = loadingMessage;

            return await operation();
        }
        catch (Exception ex)
        {
            await HandleError(ex);
            return default;
        }
        finally
        {
            IsBusy = false;
            Subtitle = string.Empty;
        }
    }

    protected virtual async Task HandleError(Exception exception)
    {
        // Log the exception
        System.Diagnostics.Debug.WriteLine($"Error in {GetType().Name}: {exception}");
        
        // Show user-friendly error message
        var errorMessage = GetUserFriendlyErrorMessage(exception);
        await ShowErrorMessage(errorMessage);
    }

    protected virtual string GetUserFriendlyErrorMessage(Exception exception)
    {
        return exception switch
        {
            HttpRequestException => "Network connection error. Please check your internet connection.",
            TaskCanceledException => "The operation timed out. Please try again.",
            UnauthorizedAccessException => "Authentication failed. Please check your credentials.",
            _ => "An unexpected error occurred. Please try again."
        };
    }

    protected virtual async Task ShowErrorMessage(string message)
    {
        // This will be implemented based on the platform's alert mechanism
        // For now, we'll use debug output
        System.Diagnostics.Debug.WriteLine($"Error: {message}");
        
        // In a real implementation, this might show a dialog or toast
        // await Application.Current.MainPage.DisplayAlert("Error", message, "OK");
    }
} 