export class Config {
    static source = "./assets"
    static destination = "../../src/OrleansChess.Web/ClientApp/src/assets/images/chesspieces"
}

interface IBrightness {
    prefix: string;
}

export interface IColor {
    friendly: string;
    hex: string;
}

class Red implements IColor {
    friendly = "red";
    hex = "#ff0000";
}

class Blue implements IColor {
    friendly = "blue";
    hex = "#0000ff";
}

class Yellow implements IColor {
    friendly = "yellow";
    hex = "#ffff00";
}

class Lime implements IColor {
    friendly = "lime";
    hex = "#32cd32"
}

class LightBlue implements IColor {
    friendly = "lightblue";
    hex = "#ADD8E6";
}

export const DarkColors: IColor[] = [
    new Red(), 
    new Blue(), 
];

export const LightColors: IColor[] = [
    new Yellow(),
    new Lime(),
    new LightBlue()
];