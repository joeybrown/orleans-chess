export class Config {
    static source = "./assets"
    static destination = "../../src/OrleansChess.Web/ClientApp/src/assets/images/chesspieces"
}

export interface IColor {
    friendly: string;
    hex: string;
    location: string
}

class Red implements IColor {
    location = "red";
    hex = "#ff0000";
    friendly = "Red";
}

class Blue implements IColor {
    location = "blue";
    hex = "#0000ff";
    friendly = "Blue";
}

class Yellow implements IColor {
    location = "yellow";
    hex = "#ffff00";
    friendly = "Yellow";
}

class Lime implements IColor {
    location = "lime";
    hex = "#32cd32";
    friendly = "Lime";
}

class LightBlue implements IColor {
    location = "lightblue";
    hex = "#ADD8E6";
    friendly = "Light Blue";
}

class White implements IColor {
    location = "white";
    hex = "#ffffff";
    friendly = "White";
}

class Black implements IColor {
    location = "black";
    hex = "#000000";
    friendly = "Black";
}

export const DarkColors: IColor[] = [
    new Red(), 
    new Blue(),
    new Black()
];

export const LightColors: IColor[] = [
    new Yellow(),
    new Lime(),
    new LightBlue(),
    new White()
];