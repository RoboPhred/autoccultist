namespace Autoccultist.Actor
{
    interface IAutoccultistAction
    {
        bool CanExecute();
        void Execute();
    }
}