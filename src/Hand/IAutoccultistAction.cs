namespace Autoccultist.Hand
{
    interface IAutoccultistAction
    {
        bool CanExecute();
        void Execute();
    }
}