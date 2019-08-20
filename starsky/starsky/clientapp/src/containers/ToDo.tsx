import * as React from "react";
import Link from '../components/Link';
import RandomJoke from "./RandomJoke";
import ToDoList from "./ToDoList";


function ToDo() {
  return (
    <div>
      <Link href="/second">To second page</Link>
      <div>My to do list</div>
      <RandomJoke />
      {/* <AddToDo /> */}
      <ToDoList />
    </div>
  );
}

export default ToDo;
