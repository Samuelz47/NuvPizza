import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterOutlet } from '@angular/router';

@Component({
  selector: 'app-root',
  standalone: true,
  // 1. Aqui importamos as ferramentas que o HTML vai usar
  imports: [CommonModule, RouterOutlet], 
  // 2. Apontamos para o seu arquivo HTML limpo
  templateUrl: './app.html',
  // 3. Apontamos para o seu CSS (conforme você disse que chama app.css)
  styleUrl: './app.css' 
})
export class AppComponent {
  title = 'NuvPizza.Web';
}