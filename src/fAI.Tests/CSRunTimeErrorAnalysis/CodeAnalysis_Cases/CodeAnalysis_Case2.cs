using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class TestUI
{
    public class ButtonCreator
    {
        public void CreateButtons()
        {
            var actions = new List<Action>();

            for (int i = 0; i < 5; i++)
            {
                actions.Add(() => Console.WriteLine($"Button {i} clicked"));
            }

            foreach (var action in actions)
            {
                action();
            }
        }
    }
}