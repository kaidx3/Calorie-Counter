namespace CalorieCounter;

public partial class MainPage : ContentPage
{
    double mealTotalCalories = 0,
        dayTotalCalories = 0,
        timelineTotalCalories = 0;
    int page = 1;
    string row1Text;
    string row2Text;
    List<string> ingredients = new List<string>();
    List<string> meals = new List<string>();
    List<string> days = new List<string>();

    string MealFilePath = $"{AppDomain.CurrentDomain.BaseDirectory}/meal-save-file.txt";
    string DayFilePath = $"{AppDomain.CurrentDomain.BaseDirectory}/day-save-file.txt";
    string TimeLineFilePath = $"{AppDomain.CurrentDomain.BaseDirectory}/timeline-save-file.txt";

    public MainPage()
    {
        InitializeComponent();
        CheckFile(MealFilePath);
        CheckFile(DayFilePath);
        CheckFile(TimeLineFilePath);
        row1Text = "Meal";
        row2Text = "Ingredient";
        UpdatePage();
    }

    //Onclick Methods
    private void OnMealClicked(object sender, EventArgs e)
    {
        page = 1;
        row1Text = "Meal";
        row2Text = "Ingredient";
        UpdatePage();
    }

    private void OnDayClicked(object sender, EventArgs e)
    {
        page = 2;
        row1Text = "Day";
        row2Text = "Meal";
        UpdatePage();
    }

    private void OnTimelineClicked(object sender, EventArgs e)
    {
        page = 3;
        row1Text = " ";
        row2Text = "Day";
        UpdatePage();
    }

    private async void OnCounterClicked(object sender, EventArgs e)
    {
        double calorie;
        double.TryParse(await Prompt("How many Calories?"), out calorie);

        switch (page)
        {
            case 2:
                if (SafeDouble(calorie, dayTotalCalories))
                {
                    dayTotalCalories += calorie;
                    meals.Add($"{calorie}");
                }
                break;
            case 3:
                if (SafeDouble(calorie, timelineTotalCalories))
                {
                    timelineTotalCalories += calorie;
                    days.Add($"{calorie}");
                }
                break;
            default:
                if (SafeDouble(calorie, mealTotalCalories))
                {
                    mealTotalCalories += calorie;
                    ingredients.Add($"{calorie}");
                }
                break;
        }
        UpdatePage();
        WriteAll();
    }

    private void OnClearClicked(object sender, EventArgs e)
    {
        switch (page)
        {
            case 2:
                dayTotalCalories = 0;
                meals.Clear();
                break;
            case 3:
                timelineTotalCalories = 0;
                days.Clear();
                break;
            default:
                mealTotalCalories = 0;
                ingredients.Clear();
                break;
        }
        UpdatePage();
        WriteAll();
    }

    private async void OnRemoveClicked(object sender, EventArgs e)
    {
        bool remove = true;
        double removeCals;
        switch (page)
        {
            case 2:
                if (meals.Count == 0)
                {
                    await DisplayAlert("Nothing to remove!", "", "Ok");
                    remove = false;
                }
                break;
            case 3:
                if (days.Count == 0)
                {
                    await DisplayAlert("Nothing to remove!", "", "Ok");
                    remove = false;
                }
                break;
            case 1:
                if (ingredients.Count == 0)
                {
                    await DisplayAlert("Nothing to remove!", "", "Ok");
                    remove = false;
                }
                break;
            default:
                remove = true;
                break;
        }
        int removeIndex;

        if (remove)
        {
            if (int.TryParse(await DisplayPromptAsync("Which number to remove?", " ", keyboard: Keyboard.Numeric), out removeIndex))
            {
                switch (page)
                {
                    case 2:
                        if (removeIndex - 1 >= 0 && removeIndex - 1 < meals.Count && meals[removeIndex - 1] != null)
                        {
                            removeCals = double.Parse(meals[removeIndex - 1]);
                            meals.RemoveAt(removeIndex - 1);
                            dayTotalCalories -= removeCals;
                        }
                        else
                        {
                            await DisplayAlert("Error", "Not a valid entry", "Ok");
                        }
                        break;
                    case 3:
                        if (removeIndex - 1 >= 0 && removeIndex - 1 < days.Count && days[removeIndex - 1] != null)
                        {
                            removeCals = double.Parse(days[removeIndex - 1]);
                            days.RemoveAt(removeIndex - 1);
                            timelineTotalCalories -= removeCals;
                        }
                        else
                        {
                            await DisplayAlert("Error", "Not a valid entry", "Ok");
                        }
                        break;
                    default:
                        if (removeIndex - 1 >= 0 && removeIndex - 1 < ingredients.Count && ingredients[removeIndex - 1] != null)
                        {
                            removeCals = double.Parse(ingredients[removeIndex - 1]);
                            ingredients.RemoveAt(removeIndex - 1);
                            mealTotalCalories -= removeCals;
                        }
                        else
                        {
                            await DisplayAlert("Error", "Not a valid entry", "Ok");
                        }
                        break;
                }
            }
        }
        UpdatePage();
        WriteAll();
    }

    private void OnFinishBtnClicked(object sender, EventArgs e)
    {
        switch (page)
        {
            case 2:
                days.Add($"{dayTotalCalories}");
                timelineTotalCalories += dayTotalCalories;
                dayTotalCalories = 0;
                meals.Clear();
                break;
            default:
                meals.Add($"{mealTotalCalories}");
                dayTotalCalories += mealTotalCalories;
                mealTotalCalories = 0;
                ingredients.Clear();
                break;
        }
        UpdatePage();
        WriteAll();
    }

    //Graphics Methods
    private void UpdatePage()
    {
        if (page == 3)
        {
            FinishBtn.IsVisible = false;
            FillerBtn.IsVisible = true;
        }
        else
        {
            FinishBtn.IsVisible = true;
            FillerBtn.IsVisible = false;
            UpdateRow1(row1Text);
        }
        switch (page)
        {
            case 2:
                CaloriesLabel.Text = $"Total Calories: {dayTotalCalories}";
                GenerateList(meals);
                break;
            case 3:
                CaloriesLabel.Text = $"Total Calories: {timelineTotalCalories}";
                GenerateList(days);
                break;
            default:
                CaloriesLabel.Text = $"Total Calories: {mealTotalCalories}";
                GenerateList(ingredients);
                break;
        }
        UpdateTotalCalories();
    }

    private void UpdateRow1(string btnText)
    {
        FinishBtn.Text = $"Complete {btnText}";
        SemanticScreenReader.Announce(FinishBtn.Text);
    }

    private void UpdateTotalCalories()
    {
        SemanticScreenReader.Announce(CaloriesLabel.Text);
    }

    private void GenerateList(List<string> list)
    {
        MealsList.Text = " ";
        for (int index = 0; index < list.Count; index++)
        {
            MealsList.Text += $"{row2Text} {index + 1}: {list[index]} calories\n";
        }
        UpdateList();
    }

    private void UpdateList()
    {
        SemanticScreenReader.Announce(MealsList.Text);
    }

    private async void ErrorMessage(string errorMessage)
    {
        await DisplayAlert("Error", errorMessage, "Ok");
    }

    private async Task<string> Prompt(string prompt)
    {
        return await DisplayPromptAsync(prompt, "", keyboard: Keyboard.Numeric);
    }


    //File Methods
    private void CheckFile(string filepath)
    {
        if (File.Exists(filepath))
        {
            StreamReader(filepath);
        }
        else
        {
            File.Create(filepath).Close();
            switch (page)
            {
                case 2:
                    StreamWriter(filepath, dayTotalCalories, meals);
                    break;
                case 3:
                    StreamWriter(filepath, timelineTotalCalories, days);
                    break;
                default:
                    StreamWriter(filepath, mealTotalCalories, ingredients);
                    break;
            }
            WriteAll();
        }
    }

    private void StreamWriter(string filepath, double totalCals, List<string> list)
    {
        StreamWriter streamWriter = new StreamWriter(filepath);

        streamWriter.WriteLine(totalCals);
        foreach (string item in list)
        {
            streamWriter.WriteLine(item);
        }
        streamWriter.Close();
    }

    private void StreamReader(string filepath)
    {
        meals.Clear();
        days.Clear();
        ingredients.Clear();
        StreamReader streamReader = new StreamReader(filepath);
        switch (page)
        {
            case 2:
                dayTotalCalories = double.Parse(streamReader.ReadLine());
                break;
            case 3:
                timelineTotalCalories = double.Parse(streamReader.ReadLine());
                break;
            default:
                mealTotalCalories = double.Parse(streamReader.ReadLine());
                break;
        }
        string line = "";
        while ((line = streamReader.ReadLine()) != null)
        {
            meals.Add(streamReader.ReadLine());
        }
        streamReader.Close();
    }

    private void WriteAll()
    {
        StreamWriter(MealFilePath, mealTotalCalories, ingredients);
        StreamWriter(DayFilePath, dayTotalCalories, meals);
        StreamWriter(TimeLineFilePath, timelineTotalCalories, days);
    }

    //Data Methods
    private bool SafeDouble(double calorie, double totalCalories)
    {
        if (calorie < 1)
        {
            ErrorMessage("Not a valid, positive number");
            return false;
        }
        if (totalCalories > 99999)
        {
            ErrorMessage("Maximum calories reached");
            return false;
        }
        else
        {
            return true;
        }
    }
}