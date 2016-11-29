open System.Drawing
    open System.Windows.Forms

    type UserEvents =
    | Dragging of MouseEventArgs
    | KeyPress of KeyEventArgs

        with
            static member CreateEvent (e1: IEvent<MouseEventArgs>, e2: IEvent<MouseEventArgs>) =
                let ev = new Event<_>()
                e1.Add(fun x -> ev.Trigger(Dragging x))
                e2.Add(fun x -> ev.Trigger(Dragging x))
                ev.Publish

            static member CreateEvent (e1: IEvent<KeyEventArgs>, e2: IEvent<KeyEventArgs>) =
                let ev = new Event<_>()
                e1.Add(fun x -> ev.Trigger(KeyPress x))
                e2.Add(fun x -> ev.Trigger(KeyPress x))
                ev.Publish