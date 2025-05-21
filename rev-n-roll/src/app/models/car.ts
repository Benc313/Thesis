export interface Car {
    id: number;
    userId: number;
    brand: string;
    model: string;
    description?: string;
    engine?: string;
    horsePower: number;
  }
  
  export interface CarRequest {
    brand: string;
    model: string;
    description?: string;
    engine?: string;
    horsePower: number;
  }