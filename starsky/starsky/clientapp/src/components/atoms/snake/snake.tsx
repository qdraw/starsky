import React, { useEffect, useRef } from "react";
enum Direction {
  Up,
  Down,
  Left,
  Right
}
type Position = {
  x: number;
  y: number;
};

class Snake {
  private readonly blockSize: number;
  private body: Position[];

  constructor(blockSize: number) {
    this.blockSize = blockSize;
    this.body = [{ x: 150, y: 150 }];
  }

  move(direction: Direction): void {
    // Add a new block to the front of the snake in the specified direction
    const newHead: Position = {
      x: this.body[0].x++,
      y: this.body[0].y++
    };
    this.body.unshift(newHead);

    // Remove the last block of the snake
    this.body.pop();
  }

  grow(): void {
    // Add a new block to the front of the snake in the same direction as the current head
    const newHead: Position = {
      x: this.body[0].x + this.body[0].x - this.body[1].x,
      y: this.body[0].y + this.body[0].y - this.body[1].y
    };
    this.body.unshift(newHead);
  }

  draw(ctx: CanvasRenderingContext2D): void {
    ctx.fillStyle = "white";
    for (const block of this.body) {
      ctx.fillRect(block.x, block.y, this.blockSize, this.blockSize);
    }
  }

  getHead(): Position {
    return this.body[0];
  }

  getBody(): Position[] {
    return this.body;
  }

  checkCollision(pos: Position, excludeHead = false): boolean {
    for (let i = excludeHead ? 1 : 0; i < this.body.length; i++) {
      if (this.body[i].x === pos.x && this.body[i].y === pos.y) {
        return true;
      }
    }
    return false;
  }
}

class Food {
  private readonly blockSize: number;
  private readonly width: number;
  private readonly height: number;
  private position: Position;

  constructor(blockSize: number, width: number, height: number) {
    this.blockSize = blockSize;
    this.width = width;
    this.height = height;
    this.position = this.randomizePosition();
  }

  randomizePosition(exclusions: Position[] = []): Position {
    // Generate a random position within the boundaries of the canvas
    let x =
      Math.floor((Math.random() * this.width) / this.blockSize) *
      this.blockSize;
    let y =
      Math.floor((Math.random() * this.height) / this.blockSize) *
      this.blockSize;

    // Check if the position collides with any of the excluded positions
    for (const exclusion of exclusions) {
      if (exclusion.x === x && exclusion.y === y) {
        return this.randomizePosition(exclusions);
      }
    }

    this.position = { x, y };
    return this.position;
  }

  draw(ctx: CanvasRenderingContext2D): void {
    ctx.fillStyle = "red";
    ctx.fillRect(
      this.position.x,
      this.position.y,
      this.blockSize,
      this.blockSize
    );
  }

  getPosition(): Position {
    return this.position;
  }
}

const SnakeGame: React.FC = () => {
  const canvasRef = useRef<HTMLCanvasElement>(null);
  const blockSize = 10;
  const direction = useRef<Direction>(Direction.Right);
  const snake = useRef(new Snake(blockSize));
  const food = useRef(
    new Food(
      blockSize,
      canvasRef.current?.width ?? 0,
      canvasRef.current?.height ?? 0
    )
  );

  const score = useRef(0);
  const intervalId = useRef<any>();

  useEffect(() => {
    // Set up event listeners for key presses
    document.addEventListener("keydown", (e) => {
      switch (e.keyCode) {
        case 37:
          if (direction.current !== Direction.Right) {
            direction.current = Direction.Left;
          }
          break;
        case 38:
          if (direction.current !== Direction.Down) {
            direction.current = Direction.Up;
          }
          break;
        case 39:
          if (direction.current !== Direction.Left) {
            direction.current = Direction.Right;
          }
          break;
        case 40:
          if (direction.current !== Direction.Up) {
            direction.current = Direction.Down;
          }
          break;
      }
    });

    // Start the game loop
    intervalId.current = setInterval(loop, 100);

    return () => clearInterval(intervalId.current);
  }, []);

  const loop = () => {
    if (
      !canvasRef.current ||
      !canvasRef.current.width ||
      !canvasRef.current.height
    ) {
      return;
    }

    const ctx = canvasRef.current.getContext("2d");
    if (!ctx) {
      return;
    }
    ctx.clearRect(0, 0, canvasRef.current.width, canvasRef.current.height);

    // Move the snake and check for collisions
    snake.current.move(direction.current);
    if (checkCollision(snake.current.getHead())) {
      return gameOver();
    }

    // Check if the snake has eaten the food
    if (snake.current.checkCollision(food.current.getPosition())) {
      score.current += 1;
      food.current.randomizePosition(snake.current.getBody());
      snake.current.grow();
    }

    // Draw the game elements to the canvas
    food.current.draw(ctx);
    snake.current.draw(ctx);
    drawScore();
  };

  const checkCollision = (pos: Position) => {
    if (!canvasRef.current) {
      return null;
    }

    // Check if the snake has collided with the walls of the canvas
    if (
      pos.x < 0 ||
      pos.x > canvasRef.current.width - blockSize ||
      pos.y < 0 ||
      pos.y > canvasRef.current.height - blockSize
    ) {
      return true;
    }

    // Check if the snake has collided with its own body
    return snake.current.checkCollision(pos, true);
  };

  const gameOver = () => {
    clearInterval(intervalId.current);
    alert(`Game over! Your score was ${score.current}`);
  };
  const drawScore = () => {
    if (!canvasRef.current) {
      return;
    }
    const ctx = canvasRef.current.getContext("2d");
    if (!ctx) {
      return;
    }
    ctx.font = "20px Arial";
    ctx.fillStyle = "white";
    ctx.textAlign = "left";
    ctx.textBaseline = "top";
    ctx.fillText(`Score: ${score.current}`, 5, 5);
  };

  return <canvas id="snake-game" ref={canvasRef} width={600} height={600} />;
};

export default SnakeGame;
